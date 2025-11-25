using UnityEngine;
using System;

public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }

    public int TotalMonsterKills { get; private set; }
    public int TotalBossKills { get; private set; }

    public int TotalKills => TotalMonsterKills + TotalBossKills;

    // KillCountUI와 연결(킬 수 변경 시 알림)
    public event Action<int> OnKillCountChanged;
    // KillCountUI와 연결(KillCounter 싱글톤 생성 시 알림)
    public static event Action<KillCounter> OnCreated;

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

    public void AddMonsterKill()
    {
        TotalMonsterKills++;
        Debug.Log("KillCounter: Monster killed. TotalMonsterKills = " + TotalMonsterKills);
        OnKillCountChanged?.Invoke(TotalKills);
    }

    public void AddBossKill()
    {
        TotalBossKills++;
        OnKillCountChanged?.Invoke(TotalKills);
    }

    public void ResetCounter()
    {
        TotalMonsterKills = 0;
        TotalBossKills = 0;
        OnKillCountChanged?.Invoke(TotalKills);
    }
}
