using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveSkillBase : ScriptableObject
{
    [Header("스킬 정보")]
    [SerializeField] private string skillName;     // 스킬 이름
    [SerializeField] private Sprite icon;          // 스킬 아이콘

    [Header("설명 템플릿")]
    [TextArea]
    [SerializeField]
    private string descriptionTemplate =
        "전방에 화염구를 발사하여 {damage}의 피해를 입힙니다.";
    // 에디터에서 이 문자열을 직접 작성할 수 있음.
    // {damage} 토큰을 우리가 수치/공식으로 치환해서 씀.

    [Header("수치")]
    [SerializeField] protected float baseValue = 10f;    // 기본 값 (예: 120)
    [SerializeField] protected float coefficient = 1.0f; // 계수 (예: 1.2 → 120% 마력)
    [SerializeField] protected float cooldown = 5f;      // 쿨타임 (초)

    [Header("시전 시간")]
    [SerializeField] protected float prepareTime = 0f;
    [SerializeField] protected float castTime = 0f;

    [Header("출력될 수 있는 애니메이션들")]
    [SerializeField] public List<AnimationType> animationTypes = new List<AnimationType>();

    private float lastUseTime = -999f;    // 마지막 사용 시각
    private float remainingCooldown = 0f; // 남은 쿨타임 (초)
    private int star = 1;                 // 성급(획득 횟수)

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
        float starMultiplier = 1f + 0.2f * (star - 1); // 중복 획득마다 20% 증가
        return baseDamage * starMultiplier;
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
    /// 스킬 획득 팝업에서 사용하기 좋음.
    /// </summary>
    public string GetDamageFormulaString()
    {
        int baseInt = Mathf.RoundToInt(baseValue);
        int coeffPerc = Mathf.RoundToInt(coefficient * 100f); // 1.2 → 120%

        return $"{baseInt} + ({coeffPerc}% Magic Stat)";
    }

    // descriptionTemplate에서 {damage}를 숫자로 치환한 "평문" 설명
    // (색 강조는 UI에서 책임진다)
    public string GetDynamicDescriptionPlain()
    {
        if (string.IsNullOrWhiteSpace(descriptionTemplate))
        {
            return $"deal {GetDamageInt()}";
        }

        string desc = descriptionTemplate.Replace("{damage}", GetDamageInt().ToString());
        return desc;
    }

    /// <summary>
    /// 획득 시 보여줄 "공식 설명" (색 없음)
    /// ex) "전방에 화염구를 발사하여 120 + (120% 마력)의 피해를 입힙니다."
    /// </summary>
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

    /// <summary>
    /// UI 쪽에서 직접 토큰 치환/색 입히고 싶을 때 쓸 수 있는 원본 템플릿
    /// (전방에 화염구를 발사하여 {damage}의 피해를 입힙니다.)
    /// </summary>
    public string GetDescriptionTemplate() => descriptionTemplate;

    // ============================
    // 쿨타임 / 사용 처리
    // ============================

    public void InitializeCooldown() => remainingCooldown = 0f;

    public bool CanUse => (remainingCooldown <= 0f);

    public void UpdateCooldown()
    {
        if (remainingCooldown > 0f)
        {
            remainingCooldown = Mathf.Max(0f, cooldown - (Time.time - lastUseTime));
        }
    }

    public bool TryUse(GameObject user, Transform target)
    {
        if (!CanUse)
            return false;

        lastUseTime = Time.time;
        remainingCooldown = cooldown;

        if (SkillManager.Instance != null)
            SkillManager.Instance.StartCoroutine(CastRoutine(user, target));

        return true;
    }

    protected abstract void Execute(GameObject user, Transform target);

    private IEnumerator CastRoutine(GameObject user, Transform target)
    {
        if (prepareTime > 0f)
            yield return new WaitForSeconds(prepareTime);

        Execute(user, target);

        if (castTime > 0f)
            yield return new WaitForSeconds(castTime);
    }

    public float GetCooldown() => remainingCooldown;

    public float GetCooldownRatio() =>
        (cooldown <= 0f) ? 0f : remainingCooldown / cooldown;

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

    public void IncreaseStar() => star++;
    public int GetNumOfStar() => star;

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
    }

    // ============================
    // 기타 정보
    // ============================

    public Sprite GetIcon() => icon;
    public string GetSkillName() => skillName;

    public override string ToString()
    {
        return skillName + star.ToString();
    }
}
