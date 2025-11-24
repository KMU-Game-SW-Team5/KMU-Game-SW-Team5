using System;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    public event Action<BossController> OnBossSpawned;
    public event Action<BossController> OnBossDied;

    public event Action<Boss2Controller> OnBoss2Spawned;
    public event Action<Boss2Controller> OnBoss2Died;

    public event Action<Boss3Controller> OnBoss3Spawned;
    public event Action<Boss3Controller> OnBoss3Died;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
