using System;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance { get; private set; }

    public event Action<BossController> OnBossSpawned;
    public event Action<BossController> OnBossDied;

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
}
