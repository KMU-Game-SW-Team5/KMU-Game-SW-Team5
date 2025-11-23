using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUIBinder : MonoBehaviour
{
    private BossController boss;
    [SerializeField] private InGameUIManager inGameUIManager;

    private void Awake()
    {
        if (inGameUIManager == null)
            inGameUIManager = GetComponent<InGameUIManager>();
    }

    private void OnEnable()
    {
        if (inGameUIManager == null || BossManager.Instance == null)
        {
            Debug.LogWarning("BossUIBinder: 참조가 비어 있습니다.");
            return;
        }

        BossManager.Instance.OnBossSpawned += HandleBossSpawned;
        BossManager.Instance.OnBossDied += HandleBossDied;
    }

    private void OnDisable()
    {
        BossManager.Instance.OnBossSpawned -= HandleBossSpawned;
        BossManager.Instance.OnBossDied -= HandleBossDied;
    }

    private void HandleHPChanged(int currentHP, int maxHP)
    {
        inGameUIManager.UpdateBossHPUI((float)currentHP, (float)maxHP);
    }

    private void HandleBossAppeared(int currentHP, int maxHP)
    {
        // string bossName = boss.bossName; // 필요한 경우 호출 후, 아래함수의 매개변수로 삽입
        inGameUIManager.AppearBossUI((float)currentHP, (float)maxHP);
    }

    private void HandleBossDisappeared()
    {
        inGameUIManager.DisappearBossUI();
    }

    private void HandleBossSpawned(BossController newBoss)
    {
        boss = newBoss;

        boss.OnHPChanged += HandleHPChanged;
        boss.OnAppeared += HandleBossAppeared;
        boss.OnDisappeared += HandleBossDisappeared;
    }

    private void HandleBossDied(BossController deadBoss)
    {
        boss.OnHPChanged -= HandleHPChanged;
        boss.OnAppeared -= HandleBossAppeared;
        boss.OnDisappeared -= HandleBossDisappeared;

        inGameUIManager.DisappearBossUI();
    }
}
