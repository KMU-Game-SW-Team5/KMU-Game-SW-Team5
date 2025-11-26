using System;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    public event Action<BossMonsterBase> OnBossMonsterSpawned;
    public event Action<BossMonsterBase> OnBossMonsterDied;

    public event Action<BossController> OnBossSpawned;
    public event Action<BossController> OnBossDied;

    public event Action<Boss2Controller> OnBoss2Spawned;
    public event Action<Boss2Controller> OnBoss2Died;

    public event Action<Boss3Controller> OnBoss3Spawned;
    public event Action<Boss3Controller> OnBoss3Died;

    // BossUIBinder와 연결(BossManager 싱글톤 생성 시 알림)
    public static event Action<BossManager> OnCreated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        OnCreated?.Invoke(this);
    }

    public void RegisterBoss(BossMonsterBase boss)
    {
        OnBossMonsterSpawned?.Invoke(boss);
    }

    public void UnregisterBoss(BossMonsterBase boss)
    {
        OnBossMonsterDied?.Invoke(boss);
    }

    public void RegisterBoss(BossController boss)
    {
        OnBossSpawned?.Invoke(boss);
    }    

    public void UnregisterBoss(BossController boss)
    {
        OnBossDied?.Invoke(boss);
    }
    

    public void RegisterBoss(Boss2Controller boss)
    {
        OnBoss2Spawned?.Invoke(boss);
    }

    public void UnregisterBoss(Boss2Controller boss)
    {
        OnBoss2Died?.Invoke(boss);
    }

    public void RegisterBoss(Boss3Controller boss)
    {
        OnBoss3Spawned?.Invoke(boss);
    }

    public void UnregisterBoss(Boss3Controller boss)
    {
        OnBoss3Died?.Invoke(boss);
    }
}
