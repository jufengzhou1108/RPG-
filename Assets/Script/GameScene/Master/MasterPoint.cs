using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 怪物生成点 —— 放置在场景中，按间隔持续从对象池生成怪物
///
/// 通过静态计数器限制全局最多 maxTotal 个怪物同时存活
/// 怪物死亡后回收入池，计时结束后自动补充
/// </summary>
public class MasterPoint : MonoBehaviour
{
    [Header("生成配置")]
    [SerializeField] private string monsterKey = "Slime";
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private int maxTotal = 5;

    private static int aliveCount;
    private List<Master> spawnedMonsters = new List<Master>();
    private float spawnTimer;

    void Start()
    {
        SpawnMonster();
        spawnTimer = 0f;
    }

    void Update()
    {
        for (int i = spawnedMonsters.Count - 1; i >= 0; i--)
        {
            if (spawnedMonsters[i] == null)
            {
                spawnedMonsters.RemoveAt(i);
                aliveCount--;
            }
        }

        if (aliveCount >= maxTotal) return;

        spawnTimer += Time.deltaTime;

        while (spawnTimer >= spawnInterval && aliveCount < maxTotal)
        {
            spawnTimer -= spawnInterval;
            SpawnMonster();
        }
    }

    private void SpawnMonster()
    {
        if (aliveCount >= maxTotal) return;

        GameObject obj = ObjectPool.Instance.Pop(monsterKey);
        if (obj == null)
        {
            Debug.LogError($"MasterPoint: ObjectPool.Pop [{monsterKey}] 返回 null");
            return;
        }

        Master master = obj.GetComponent<Master>();
        if (master == null)
        {
            Debug.LogError($"MasterPoint: 预制体 [{monsterKey}] 缺少 Master 脚本");
            ObjectPool.Instance.Push(monsterKey, obj);
            return;
        }

        obj.transform.position = transform.position;
        obj.transform.rotation = transform.rotation;

        master.OnDeath += HandleMonsterDeath;
        spawnedMonsters.Add(master);
        aliveCount++;
    }

    private void HandleMonsterDeath(Master dead)
    {
        dead.OnDeath -= HandleMonsterDeath;
        spawnedMonsters.Remove(dead);
        aliveCount--;

        TaskManager.Instance.AddProgress("Task1", "1", 1);

        ObjectPool.Instance.Push(monsterKey, dead.gameObject);
    }

    void OnDestroy()
    {
        foreach (Master m in spawnedMonsters)
        {
            if (m != null) m.OnDeath -= HandleMonsterDeath;
        }
    }
}
