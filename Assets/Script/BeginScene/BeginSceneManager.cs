using UnityEngine;

/// <summary>
/// 开始场景管理器 —— 管理开始场景的生命周期
/// 进入场景时通过 ViewManager 加载开始界面
/// </summary>
public class BeginSceneManager : MonoBehaviour
{
    private bool isViewLoaded;

    void Start()
    {
        // 进入场景时加载开始界面
        ShowBeginView();
    }

    /// <summary>
    /// 显示开始界面
    /// </summary>
    private void ShowBeginView()
    {
        if (isViewLoaded)
        {
            return;
        }

        ViewManager.Instance.Show<BeginView>();
        isViewLoaded = true;
    }

    /// <summary>
    /// 隐藏开始界面（场景切换时调用）
    /// </summary>
    public void HideBeginView()
    {
        if (!isViewLoaded)
        {
            return;
        }

        ViewManager.Instance.Hide<BeginView>();
        isViewLoaded = false;
    }

    void OnDestroy()
    {
        // 场景销毁时清理界面
        HideBeginView();
    }
}
