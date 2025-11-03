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

    // TODO : 플레이어 스탯 컴포넌트 생기면 연동 시킬 것
    // [serializeField] private PlayerStats;
    public float magicStat = 1f;          // 플레이어의 마력 스탯

    private float lastUseTime = -999f;                    // 마지막 사용 시각
    private float remainingCooldown = 0f;                 // 남은 쿨타임 (초)


    // TODO : 플레이어 스탯 컴포넌트 만들어지면 마력 업데이트 하는 함수 완성할 것
    //public void UpdateMagicStat()
    //{
    //    magicStat = PlayerStats.GetMagicStat();
    //}

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

    // 스킬 사용 시도 — 사용 가능하면 실행
    public void TryUse(GameObject user)
    {
        // 아직 쿨이면 실행이 안 되고, UI에 쿨이라고 표시 (UI 만들어지면 완성할 것)
        if (!CanUse)
        {
            // TODO : UI에 재사용 대기 중이라고 띄우기
            return;    
        }
        lastUseTime = Time.time;        // 사용 시간 기록
        remainingCooldown = cooldown;   // 쿨타임 적용
        Execute(user);                  // 스킬 실행
    }


    /// 실제 스킬 효과 구현 (파생 클래스에서 반드시 정의)
    protected abstract void Execute(GameObject user);

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

    // TODO : 스킬 설명 업데이트하는 함수 만들 것
}
