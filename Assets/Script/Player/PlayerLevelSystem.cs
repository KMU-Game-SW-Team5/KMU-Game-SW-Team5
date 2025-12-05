using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLevelSystem : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static PlayerLevelSystem Instance { get; private set; }

    [Header("초기 설정")]
    [SerializeField] private int startLevel = 1;

    [Header("경험치 증가 규칙")]
    [SerializeField] private int baseRequiredExp = 1000;   // 1 -> 2 필요 경험치
    [SerializeField] private int expIncrementPerLevel = 200; // 레벨당 추가 경험치

    [Header("마력 증가 규칙")]
    [SerializeField] private int baseMagicStat = 1000;
    [SerializeField] private int magicStatIncrementPerLevel = 10;

    [Header("최대 체력 증가 규칙")]
    [SerializeField] private int baseMaxHp = 100;
    [SerializeField] private int maxHpIncrementPerLevel = 10;

    public int Level { get; private set; }
    public int CurrentExp { get; private set; }

    /// <summary>다음 레벨까지 필요한 경험치</summary>
    public int RequiredExp
    {
        get => baseRequiredExp + (Level - 1) * expIncrementPerLevel;
    }

    // PlayerUIBinder와 연결
    /// <summary>경험치 변화 시 호출(현재 경험치, 다음 레벨까지 필요 경험치)</summary>
    public event Action<float, float> OnExpChanged;

    /// <summary>레벨업 시 호출</summary>
    public event Action<int> OnLevelUp;

    private void Awake()
    {
        Level = startLevel;
        CurrentExp = 0;

        // 싱글톤 기본 코드
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        OnLevelUp += HandleLevelUp;
        SkillManager.Instance.SetMagicStat(baseMagicStat);
        Player.Instance.SetMaxHp(baseMaxHp);
    }

    private void OnDestroy()
    {
        OnLevelUp -= HandleLevelUp;
    }

    [SerializeField] private AudioClip levelUpSFX;

    /// <summary>플레이어가 amount 만큼 경험치를 얻음</summary>
    public void AddExp(int amount)
    {
        if (amount <= 0f) return;

        CurrentExp += amount;

        while (CurrentExp >= RequiredExp)
        {
            CurrentExp -= RequiredExp;
            Level++;
            IncreaseMagicStat();
            IncreaseMaxHp();
            SFX_Manager.Instance.PlayOneShot(levelUpSFX);
            OnLevelUp?.Invoke(Level);
        }

        OnExpChanged?.Invoke(CurrentExp, RequiredExp);
    }

    private void HandleLevelUp(int newLevel)
    {
        // 레벨업 UI 띄우기
        InGameUIManager.Instance.ShowLevelUpUI();
    }

    private void IncreaseMagicStat()
    {
        SkillManager.Instance.AddMagicStat(magicStatIncrementPerLevel);
    }

    private void IncreaseMaxHp()
    {
        Player.Instance.AddMaxHp(maxHpIncrementPerLevel);
    }
}
