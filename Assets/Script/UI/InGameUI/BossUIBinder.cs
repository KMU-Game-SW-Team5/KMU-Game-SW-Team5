using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossUIBinder : MonoBehaviour
{
    private BossController boss;

    private void OnEnable()
    {
        if (BossManager.Instance == null)
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
        InGameUIManager.Instance.UpdateBossHPUI((float)currentHP, (float)maxHP);
    }

    private void HandleBossAppeared(int currentHP, int maxHP)
    {
        // string bossName = boss.bossName; // 필요한 경우 호출 후, 아래함수의 매개변수로 삽입
        InGameUIManager.Instance.AppearBossUI((float)currentHP, (float)maxHP);
    }

    private void HandleBossDisappeared()
    {
        InGameUIManager.Instance.DisappearBossUI();
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

        InGameUIManager.Instance.DisappearBossUI();
    }
}
