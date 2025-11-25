using UnityEngine;
using System;

public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }

    public int TotalMonsterKills { get; private set; }
    public int TotalBossKills { get; private set; }

    public int TotalKills => TotalMonsterKills + TotalBossKills;

    // �� ų ���� ���� ������ ȣ��
    public event Action<int> OnKillCountChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
