using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveSkillBase : ScriptableObject
{
    private const int MaxStar = 3;   // ⭐ 최대 성급 3성

    [Header("스킬 정보")]
    [SerializeField] private string skillName;     // 스킬 이름
    [SerializeField] private Sprite icon;          // 스킬 아이콘

    [Header("설명 템플릿")]
    [TextArea]
    [SerializeField]
    private string descriptionTemplate =
        "Launches a fireball in the target direction that deals {damage} to the first enemy hit.";

    [Header("수치")]
    [SerializeField] protected float baseValue = 10f;    // 기본 값 (예: 120)
    [SerializeField] protected float coefficient = 1.0f; // 계수 (예: 1.2 → 120% 마력)

    [SerializeField] protected float baseCooldown = 5f;              // 설정된 기본 쿨타임
    [SerializeField] protected float cooldownDecreasePerStar = 1.0f; // 성급당 쿨타임 감소량
    protected float currentCooldown = 5f;                            // 런타임에서 쓰이는 쿨타임

    [Header("성급 데미지 보너스")]
    [Tooltip("2성에서 추가되는 데미지 비율 (0.4 = +40%)")]
    [SerializeField] private float secondStarBonus = 0.4f; // 40%

    [Tooltip("3성에서 추가되는 데미지 비율 (1.0 = +100%)")]
    [SerializeField] private float thirdStarBonus = 1.0f;  // 100%

    [Header("시전 시간")]
    [SerializeField] protected float prepareTime = 0f;
    [SerializeField] protected float castTime = 0f;

    [Header("출력될 수 있는 애니메이션들")]
    [SerializeField] public List<AnimationType> animationTypes = new List<AnimationType>();

    [Header("사운드")]
    [SerializeField] public AudioClip castClip;

    [Header("화면 이팩트")]
    [SerializeField] private Color screenFlashColor = new Color(0f, 0f, 0f, 0f);
    [SerializeField] private float screenFlashDuration = 0f;
    [SerializeField] private float screenFlashPeriod = 0f;

    [Header("카메라 흔들기")]
    [SerializeField] private float camShakeDelay = 0f;      // 카메라 흔들기 지연 시간
    [SerializeField] private float camShakeIntensity = 0f; // 카메라 흔들기 세기(0이면 안 흔듬)
    [SerializeField] private float camShakeDuration = 0f;  // 카메라 흔들기 지속 시간

    private float lastUseTime = -999f;    // 마지막 사용 시각
    private float remainingCooldown = 0f; // 남은 쿨타임 (초)
    private int star = 1;                 // 성급(획득 횟수) – 1~3

    // ============================
    // 데미지 / 수식 계산
    // ============================

    /// <summary>
    /// 현재 마력/성급을 반영한 실제 데미지 (float)
    /// </summary>
    public float GetDamage()
    {
        float magicStat = SkillManager.Instance.GetMagicStat();
        float baseDamage = baseValue + magicStat * coefficient;
        float starMultiplier = GetStarDamageMultiplier();
        return baseDamage * starMultiplier;
    }

    /// <summary>
    /// 성급에 따른 데미지 배율:
    /// 1성: 1.0
    /// 2성: 1.0 + secondStarBonus (기본 1.4)
    /// 3성: 1.0 + thirdStarBonus  (기본 2.0)
    /// </summary>
    private float GetStarDamageMultiplier()
    {
        int clampedStar = Mathf.Clamp(star, 1, MaxStar);

        if (clampedStar == 1)
            return 1f;
        if (clampedStar == 2)
            return 1f + secondStarBonus;
        // 3성 이상은 전부 3성 취급
        return 1f + thirdStarBonus;
    }

    /// <summary>
    /// UI에서 쓰기 쉬운 정수 데미지
    /// </summary>
    public int GetDamageInt()
    {
        return Mathf.RoundToInt(GetDamage());
    }

    /// <summary>
    /// "120 + (120% 마력)" 이런 형태의 공식 문자열 (성급은 반영 X, base+계수만)
    /// </summary>
    public string GetDamageFormulaString()
    {
        int baseInt = Mathf.RoundToInt(baseValue);
        int coeffPerc = Mathf.RoundToInt(coefficient * 100f); // 1.2 → 120%

        return $"{baseInt} + ({coeffPerc}% Magic Stat)";
    }

    public string GetDynamicDescriptionPlain()
    {
        if (string.IsNullOrWhiteSpace(descriptionTemplate))
        {
            return $"deal {GetDamageInt()}";
        }

        string desc = descriptionTemplate.Replace("{damage}", GetDamageInt().ToString());
        return desc;
    }

    public string GetAcquisitionDescriptionPlain()
    {
        string formula = GetDamageFormulaString();

        if (string.IsNullOrWhiteSpace(descriptionTemplate))
        {
            return $"deal {formula} damage";
        }

        string desc = descriptionTemplate.Replace("{damage}", formula);
        return desc;
    }

    public string GetDescriptionTemplate() => descriptionTemplate;

    // ============================
    // 쿨타임 / 사용 처리
    // ============================

    public void InitializeCooldown() => remainingCooldown = 0f;

    public bool CanUse()
    {
        return remainingCooldown <= 0f;
    }

    public void UpdateCooldown()
    {
        if (remainingCooldown > 0f)
        {
            remainingCooldown = Mathf.Max(0f, currentCooldown - (Time.time - lastUseTime));
        }
    }

    public bool TryUse(GameObject user, Transform target, bool ignoreCooldown = false)
    {
        if (!ignoreCooldown && !CanUse())
            return false;

        lastUseTime = Time.time;
        remainingCooldown = currentCooldown;

        if (SkillManager.Instance != null)
            SkillManager.Instance.StartCoroutine(CastRoutine(user, target));

        return true;
    }

    protected abstract void Execute(GameObject user, Transform target);

    private IEnumerator CastRoutine(GameObject user, Transform target)
    {
        if (prepareTime > 0f)
            yield return new WaitForSeconds(prepareTime);

        // 카메라 흔들기가 설정되어 있다면 흔듬
        if (camShakeIntensity > 0f && camShakeDuration > 0f)
        {
            // 지연 후 카메라 흔들기
            SkillManager.Instance.StartCoroutine(PlayCameraShakeAfterDelay());
        }

        // 화면 효과가 있다면 적용
        if (screenFlashColor.a > 0f && screenFlashDuration > 0f)
        {
            Debug.Log("ActiveSkillBase: 화면 플래시 효과 재생");
            CombatUIManager.Instance?.PlayScreenColorEffect(
                screenFlashDuration,
                screenFlashColor,
                screenFlashPeriod
            );
        }

        // 스킬 실행
        Execute(user, target);

        if (castTime > 0f)
            yield return new WaitForSeconds(castTime);
    }

    public float GetCooldown() => remainingCooldown;

    public float GetCooldownRatio() =>
        (currentCooldown <= 0f) ? 0f : remainingCooldown / currentCooldown;

    public void DecreaseCooldown(float sec)
    {
        remainingCooldown = Mathf.Max(0, remainingCooldown - sec);
    }

    public void ClearCooldown() => remainingCooldown = 0f;

    // ============================
    // 수치 조정 / 성급
    // ============================

    public void SetBaseValue(float value) => baseValue = value;
    public void IncreaseBaseValue(float value) => baseValue += value;
    public void IncreaseCoefficient(float value) => coefficient += value;

    /// <summary>
    /// 성급을 1 증가시키되, 최대 3성까지.
    /// 쿨타임은 성급에 따라 다시 계산.
    /// </summary>
    public void IncreaseStar()
    {
        if (star >= MaxStar)
            return;

        star++;
        RecalculateCurrentCooldown();
    }

    public int GetNumOfStar() => star;

    public void ClearStar()
    {
        star = 1;
        RecalculateCurrentCooldown();
    }

    private void RecalculateCurrentCooldown()
    {
        // 1성: baseCooldown
        // 2성: baseCooldown - 1 * cooldownDecreasePerStar
        // 3성: baseCooldown - 2 * cooldownDecreasePerStar
        int levelOffset = Mathf.Clamp(star - 1, 0, MaxStar - 1);
        currentCooldown = Mathf.Max(0.2f, baseCooldown - cooldownDecreasePerStar * levelOffset);
    }

    // ============================
    // 애니메이션
    // ============================

    public AnimationType GetSkillAnimation()
    {
        if (animationTypes == null || animationTypes.Count == 0)
        {
            Debug.LogWarning($"스킬 [{skillName}]에 애니메이션이 할당되지 않았습니다.");
            return AnimationType.Idle;
        }

        int index = Random.Range(0, animationTypes.Count);
        return animationTypes[index];
    }

    public float GetPrepareTime() => prepareTime;
    public float GetCastTime() => castTime;

    // 초기화
    public virtual void Initialize()
    {
        star = 1;
        RecalculateCurrentCooldown();
        InitializeCooldown();
    }

    // ============================
    // 기타 정보
    // ============================

    public Sprite GetIcon() => icon;
    public string GetSkillName() => skillName;

    public override string ToString()
    {
        int clampedStar = Mathf.Clamp(star, 1, MaxStar);
        return skillName + clampedStar.ToString();
    }

    // 지정한 시간 후 카메라 흔들기
    private IEnumerator PlayCameraShakeAfterDelay()
    {
        yield return new WaitForSeconds(camShakeDelay);
        CombatUIManager.Instance.PlayCameraShake(camShakeIntensity, camShakeDuration);
    }
}
