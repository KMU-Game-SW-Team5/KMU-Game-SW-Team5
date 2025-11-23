using UnityEngine;

public class KillCounter : MonoBehaviour
{
    public static KillCounter Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public int TotalMonsterKills { get; private set; }
    public int TotalBossKills { get; private set; }

    public int TotalKills => TotalMonsterKills + TotalBossKills;

    public void AddMonsterKill()
    {
        TotalMonsterKills++;
        Debug.Log($"Monster Kills: {TotalMonsterKills}");
    }

    public void AddBossKill()
    {
        TotalBossKills++;
        Debug.Log($"Boss Kills: {TotalBossKills}");
    }

    public void ResetCounter()
    {
        TotalMonsterKills = 0;
        TotalBossKills = 0;
    }
}
