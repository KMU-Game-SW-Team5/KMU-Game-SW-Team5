using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ActiveSkillBase : ScriptableObject
{
    [Header("스킬 정보")]
    [SerializeField] private string skillName;            // 스킬 이름
    [SerializeField] private Sprite icon;                 // 스킬 아이콘
    [SerializeField] private string describe;             // 스킬 설명
    [SerializeField] protected float baseValue = 10f;     // 기본 수치
    [SerializeField] protected float coefficient = 1.0f;  // (마력) 계수
    [SerializeField] protected float cooldown = 5f;       // 쿨타임 (초)


    [Header("시전 시간")]
    [Tooltip("스킬 버튼을 누르고 실제 효과(Execute)가 발생하기까지의 준비 시간(초)")]
    [SerializeField] protected float prepareTime = 0f;

    [Tooltip("실제 시전 모션/채널링이 유지되는 시간(초). 필요에 따라 외부에서 사용")]
    [SerializeField] protected float castTime = 0f;

    [Header("출력될 수 있는 애니메이션들")]
    [SerializeField] public List<AnimationType> animationTypes = new List<AnimationType>();


    private float lastUseTime = -999f;                    // 마지막 사용 시각
    private float remainingCooldown = 0f;                 // 남은 쿨타임 (초)

    private int star = 1;                   // 성급(획득 횟수)

    // 플레이어의 마력 스탯을 가져와서 스킬의 데미지 출력
    public float GetDamage()
    {
        float magicStat = SkillManager.Instance.GetMagicStat();
        return (baseValue + magicStat * coefficient) * (1 + 0.2f * (float) (star-1));   // 중복 획득마다 20% 증가
    }

    // skill manager에서 초기화함.
    public void Init()
    {
        remainingCooldown = 0f;
    }
    // 현재 쿨타임이 끝났는지 여부
    public bool CanUse => (remainingCooldown <= 0f);

    // 매 프레임 호출하여 쿨타임 감소 계산 (SkillManager.Update에서 호출)
    public void UpdateCooldown()
    {
        if (remainingCooldown > 0f)
        {
            remainingCooldown = Mathf.Max(0f, cooldown - (Time.time - lastUseTime));
        }
    }

    // 스킬 사용 시도 — 사용 가능하면 Execute호출 후 true리턴, 실패하면 false리턴
    public bool TryUse(GameObject user, Transform target)
    {
        // 아직 쿨이면 실행이 안 되고, UI에 쿨이라고 표시 (UI 만들어지면 완성할 것)
        if (!CanUse)
        {
            // TODO : UI에 재사용 대기 중이라고 띄우기
            return false;    
        }
        lastUseTime = Time.time;        // 사용 시간 기록
        remainingCooldown = cooldown;   // 쿨타임 적용

        // 준비 시간이 지난 뒤에 Execute를 호출하도록 코루틴 실행
        if (SkillManager.Instance != null)
             SkillManager.Instance.StartCoroutine(CastRoutine(user, target));

        return true;
    }


    /// 실제 스킬 효과 구현 (파생 클래스에서 반드시 정의)
    protected abstract void Execute(GameObject user, Transform target);
    private IEnumerator CastRoutine(GameObject user, Transform target)
    {
        // 🔹 준비 시간 대기
        if (prepareTime > 0f)
            yield return new WaitForSeconds(prepareTime);

        // 🔹 실제 스킬 효과 발동
        Execute(user, target);

        // 🔹 castTime은 "시전 유지 시간"으로, 필요 시 외부에서 이 값을 보고
        //     이동/입력 제한 등을 걸 수 있게 남겨둠.
        if (castTime > 0f)
            yield return new WaitForSeconds(castTime);

        // 여기서 castTime이 끝난 뒤에 뭔가를 해야 한다면
        // (예: 시전 해제, 상태 복구 등) 추후 확장 가능.
    }


    /// 기본 수치 + 마력 * 계수 계산
    public float GetPower(float userMagicStat)
    {
        return baseValue + userMagicStat * coefficient;
    }

    // 남은 쿨타임 리턴
    public float GetCooldown()
    {
        return remainingCooldown;
    }

    // 남은 쿨타임 비율(0~1) 리턴
    public float GetCooldownRatio()
    {
        return (remainingCooldown / cooldown);
    }

    // 쿨타임 입력한 만큼 감소(음수 넣으면 증가)
    public void DecreaseCooldown(float sec)
    {
        remainingCooldown = Mathf.Max(0, remainingCooldown - sec);
    }

    // 쿨타임 0으로 초기화
    public void ClearCooldown()
    {
        remainingCooldown = 0f;
    }

    // 스킬 기본 값 재설정
    public void SetBaseValue (float value)
    {
        baseValue = value;
    }

    // 스킬 기본 값 증가(음수 넣으면 감소)
    public void IncreaseBaseValue(float value)
    {
        baseValue += value;
    }

    // 스킬 계수 증가
    public void IncreaseCoefficient(float value)
    {
        coefficient += value;
    }

    // 스킬 아이콘 getter
    public Sprite GetIcon()
    {
        return icon;
    }

    // 성급 증가. 중복 획득시 호출
    public void IncreaseStar()
    {
        star++;
    }

    // 스킬 애니메이션 리턴
    public AnimationType GetSkillAnimation()
    {
        if (animationTypes == null || animationTypes.Count == 0)
        {
            Debug.LogWarning($"스킬 [{skillName}]에 애니메이션이 할당되지 않았습니다.");
            return AnimationType.Idle;   // 안전한 기본값
        }

        int index = Random.Range(0, animationTypes.Count); // 0 ~ Count-1
        return animationTypes[index];
    }


    // 준비 시간/시전 시간 Getter (원하면 외부에서 참고 가능)
    public float GetPrepareTime() => prepareTime;
    public float GetCastTime() => castTime;

    // TODO : 스킬 설명 업데이트하는 함수 만들 것
}
