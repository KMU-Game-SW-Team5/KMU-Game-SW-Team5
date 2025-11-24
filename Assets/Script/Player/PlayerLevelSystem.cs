using System;
using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
    [Header("초기 설정")]
    [SerializeField] private int startLevel = 1;

    [Header("경험치 증가 규칙")]
    [SerializeField] private float baseRequiredExp = 100f;   // 1 -> 2 필요 경험치
    [SerializeField] private float expIncrementPerLevel = 50f; // 레벨당 추가 경험치

    public int Level { get; private set; }
    public float CurrentExp { get; private set; }

    /// <summary>다음 레벨까지 필요한 경험치</summary>
    public float RequiredExp
    {
        get => baseRequiredExp + (Level - 1) * expIncrementPerLevel;
    }

    // UI나 다른 시스템이 구독할 이벤트
    /// <summary>경험치 변화 시 호출(현재 경험치, 다음 레벨까지 필요 경험치)</summary>
    public event Action<float, float> OnExpChanged;

    /// <summary>레벨업 시 호출</summary>
    public event Action<int> OnLevelUp;

    private void Awake()
    {
        Level = startLevel;
        CurrentExp = 0f;
    }

    /// <summary>플레이어가 amount 만큼 경험치를 얻음</summary>
    public void AddExp(float amount)
    {
        if (amount <= 0f) return;

        CurrentExp += amount;

        while (CurrentExp >= RequiredExp)
        {
            CurrentExp -= RequiredExp;
            Level++;

            OnLevelUp?.Invoke(Level);
        }

        OnExpChanged?.Invoke(CurrentExp, RequiredExp);
    }
}
