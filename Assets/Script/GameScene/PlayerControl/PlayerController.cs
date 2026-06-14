using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 玩家控制脚本 —— 使用 PlayerInput 读取移动输入，通过 CharacterController 控制玩家移动
/// 按 E 键对最近的对话对象发起对话，鼠标左键朝鼠标指向方向发射子弹
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("玩家配置")]
    [SerializeField] private PlayerConfig playerConfig;        // 玩家配置资产（ScriptableObject）

    [Header("对话检测")]
    [SerializeField] private float detectionInterval = 0.15f;   // 检测间隔（秒）
    [SerializeField] private float detectionRadius = 1f;        // 检测半径（米）

    [Header("射击")]
    [SerializeField] private string bulletKey = "Bullet";       // 对象池 / Addressable 键名
    [SerializeField] private float fireInterval = 0.3f;         // 射击间隔（秒）

    // 输入系统
    private PlayerInput playerInput;                            // 输入动作资产实例
    private InputAction moveAction;                             // Move 动作引用
    private InputAction talkAction;                             // Talk 动作引用
    private Vector2 moveInput;                                  // 当前移动输入

    // 组件
    private CharacterController characterController;            // 角色控制器

    // 移动速度（运行时从配置读取）
    private float moveSpeed;

    // 玩家血量（从配置读取，受击时扣减并通过事件通知 GameView）
    private float maxHP;
    private float currentHP;

    // 对话检测
    private LayerMask dialogableMask;                           // Dialogable 层遮罩
    private float detectionTimer;                               // 检测计时器
    private DialogueObject currentDialogueObject;               // 当前检测到的对话对象

    // 射击
    private Camera mainCamera;                                  // 主相机缓存
    private float fireTimer;                                    // 射击冷却计时器
    private Plane groundPlane = new Plane(Vector3.up, 0f);     // 水平面（复用以减少 GC）

    void Awake()
    {
        // 从配置资产读取参数，未配置则使用默认值
        if (playerConfig != null)
        {
            moveSpeed = playerConfig.moveSpeed;
            maxHP = playerConfig.maxHP;
            currentHP = maxHP;
        }
        else
        {
            moveSpeed = 5f;
            maxHP = 100f;
            currentHP = maxHP;
            Debug.LogWarning("PlayerController: PlayerConfig 未配置，使用默认值");
        }

        // 获取组件
        characterController = GetComponent<CharacterController>();

        // 实例化输入动作资产
        playerInput = new PlayerInput();

        // 缓存输入动作引用
        moveAction = playerInput.Player.Move;
        talkAction = playerInput.Player.Talk;

        // 对话检测层遮罩
        dialogableMask = LayerMask.GetMask("Dialogable");

        // 缓存主相机
        mainCamera = Camera.main;

        // 通过事件中心订阅 GameView 显隐事件，控制玩家输入开关
        EventCenter.Instance.AddListener<GameViewShowEvent>(OnGameViewShow);
        EventCenter.Instance.AddListener<GameViewHideEvent>(OnGameViewHide);

        // 订阅 Talk 动作回调
        if (talkAction != null)
        {
            talkAction.performed += OnTalkPerformed;
        }
    }

    /// <summary>
    /// GameView 显示 → 启用玩家输入，同步当前血量给 GameView
    /// </summary>
    private void OnGameViewShow(GameViewShowEvent _)
    {
        playerInput?.Enable();
        FireHPUpdateEvent();
    }

    /// <summary>
    /// GameView 隐藏 → 关闭玩家输入
    /// </summary>
    private void OnGameViewHide(GameViewHideEvent _)
    {
        playerInput?.Disable();
    }

    void Update()
    {
        // 从 Player 动作映射表读取 Move 输入值（WASD 的 Vector2）
        moveInput = moveAction.ReadValue<Vector2>();

        // 执行移动
        ApplyMovement();

        // 对话检测计时
        detectionTimer += Time.deltaTime;
        if (detectionTimer >= detectionInterval)
        {
            detectionTimer = 0f;
            DetectDialogueObjects();
        }

        // 射击冷却计时
        if (fireTimer > 0f)
        {
            fireTimer -= Time.deltaTime;
        }

        // 鼠标左键射击（仅在输入未关闭时生效）
        if (Mouse.current.leftButton.wasPressedThisFrame && fireTimer <= 0f && playerInput.Player.enabled)
        {
            TryShoot();
        }
    }

    #region 射击

    /// <summary>
    /// 鼠标左键点击 → 从屏幕鼠标位置发射射线，与玩家脚底水平面求交，
    /// 得到方向后从对象池取出子弹发射
    /// </summary>
    private void TryShoot()
    {
        if (mainCamera == null) return;

        // 1. 在地面高度（Y=0）构造水平面
        groundPlane.SetNormalAndPosition(Vector3.up, Vector3.zero);

        // 2. 从屏幕鼠标位置发射射线
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // 3. 射线与地面求交
        if (!groundPlane.Raycast(ray, out float enter) || enter < 0f) return;

        Vector3 hitPoint = ray.GetPoint(enter);

        // 4. 纯水平方向
        Vector3 direction = hitPoint - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.01f) return;

        // 5. 从对象池取子弹，Fire 传入 key（保证 Push 用同一个 key）
        GameObject bulletObj = ObjectPool.Instance.Pop(bulletKey);
        bulletObj.transform.position = transform.position;
        bulletObj.GetComponent<Bullet>().Fire(direction, bulletKey);

        fireTimer = fireInterval;
    }

    #endregion

    #region 对话触发

    /// <summary>
    /// E 键按下 → 对最近的对话对象发起对话
    /// </summary>
    private void OnTalkPerformed(InputAction.CallbackContext context)
    {
        if (currentDialogueObject == null) return;

        DialogueConfig config = currentDialogueObject.Config;
        if (config == null || config.dialogues == null || config.dialogues.Length == 0)
        {
            Debug.LogWarning("PlayerController: 对话对象的 DialogueConfig 为空或无对话条目");
            return;
        }

        // 设置配置 → 显示对话 UI → 开始对话
        TalkManager.Instance.SetConfig(config);
        ViewManager.Instance.Show<TalkView>();
        TalkManager.Instance.StartDialogue(config.dialogues[0].dialogueId);
    }

    #endregion

    /// <summary>
    /// 每 detectionInterval 秒对周围 detectionRadius 米进行 Dialogable 层范围检测
    /// </summary>
    private void DetectDialogueObjects()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, dialogableMask);

        DialogueObject found = null;
        float closestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            DialogueObject dialogueObj = hit.GetComponent<DialogueObject>();
            if (dialogueObj != null)
            {
                float dist = Vector3.Distance(transform.position, hit.ClosestPoint(transform.position));
                if (dist < closestDist)
                {
                    closestDist = dist;
                    found = dialogueObj;
                }
            }
        }

        currentDialogueObject = found;
    }

    /// <summary>
    /// 当前范围内可交互的对话对象（无则 null）
    /// </summary>
    public DialogueObject CurrentDialogueObject => currentDialogueObject;

    #region 血量

    /// <summary>
    /// 受到伤害，扣减血量并通过事件通知 GameView 刷新血条
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHP <= 0f) return;

        currentHP = Mathf.Max(0f, currentHP - damage);
        FireHPUpdateEvent();
    }

    /// <summary>
    /// 发送血量更新事件到 EventCenter
    /// </summary>
    private void FireHPUpdateEvent()
    {
        EventCenter.Instance.EventTrigger(new PlayerHPUpdateEvent
        {
            currentHP = this.currentHP,
            maxHP = this.maxHP
        });
    }

    #endregion

    /// <summary>
    /// 使用 CharacterController.SimpleMove 根据输入值移动玩家
    /// SimpleMove 自动处理重力，忽略 Y 分量，内部使用 Time.deltaTime
    /// </summary>
    private void ApplyMovement()
    {
        if (moveInput == Vector2.zero) return;

        // 将 2D 输入转换为 3D 世界移动方向
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        movement = movement.normalized * moveSpeed;

        // SimpleMove: 世界空间移动，自动施加重力，速度无需乘 deltaTime
        characterController.SimpleMove(movement);
    }

    void OnDestroy()
    {
        // 取消事件中心订阅
        EventCenter.Instance.RemoveListener<GameViewShowEvent>(OnGameViewShow);
        EventCenter.Instance.RemoveListener<GameViewHideEvent>(OnGameViewHide);

        // 取消 Talk 动作回调
        if (talkAction != null)
        {
            talkAction.performed -= OnTalkPerformed;
        }

        playerInput?.Dispose();
    }
}
