using UnityEngine;

[CreateAssetMenu(fileName = "AS_Buff", menuName = "Scriptable Object/Active Skill/Buff")]
public class AS_BuffType : ActiveSkillBase
{
    [Header("버프 설정")]
    [SerializeField] private BuffStatType targetStat;  // 어떤 스탯을 올릴지
    [SerializeField] private bool isPermanent = false;   // 영구 지속인 경우
    [SerializeField] private float buffDuration = 5f;  // 버프 지속 시간(초)
    [SerializeField] private bool usePercent = false;  // true면 % 단위, false면 절대값


    protected override void Execute(GameObject user, Transform target)
    {
        if (user == null) return;

        // 마력, 성급이 반영된 damage 값
        float rawValue = GetDamage();

        float buffAmount = rawValue;

        // 만약 % 버프로 쓰고 싶다면 예: rawValue = 200 → 200% = 2.0배
        if (usePercent)
        {
            buffAmount = rawValue * 0.01f; // 200 -> 2.0 (200%)
        }

        if (isPermanent)
        {
            SkillManager.Instance.buffApplier.ApplyBuff(targetStat, buffAmount);
        } else
        {
            Debug.Log($"AS_BuffType: Applying buff {targetStat} +{buffAmount} for {buffDuration} seconds.");
            SkillManager.Instance.buffApplier.ApplyBuffFor(targetStat, buffAmount, buffDuration);
        }


        // 필요하면 여기서 버프용 이펙트, 사운드, UI 표시 등을 호출해도 됨
        // 예: CombatUIManager.Instance?.ShowBuffIcon(this, buffDuration);
    }
}
