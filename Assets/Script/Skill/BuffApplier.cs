using System.Collections;
using UnityEngine;

public class BuffApplier : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private MoveController moveController;
    // 필요하면 Player, 방어력 컴포넌트 등도 추가
    // [SerializeField] private PlayerDefense playerDefense;

    private void Awake()
    { 
        if (moveController == null)
            moveController = GetComponent<MoveController>();
    }

    public void ApplyBuff(BuffStatType statType, float amount, float duration)
    {
        StartCoroutine(BuffRoutine(statType, amount, duration));
    }

    private IEnumerator BuffRoutine(BuffStatType statType, float amount, float duration)
    {
        // 버프 적용
        AddBuff(statType, amount);

        yield return new WaitForSeconds(duration);

        // 버프 해제 (같은 양을 반대로 적용)
        AddBuff(statType, -amount);
    }

    private void AddBuff(BuffStatType statType, float amount)
    {
        switch (statType)
        {
            case BuffStatType.Magic:
                SkillManager.Instance.AddMagicStat(amount);
                break;


            //case BuffStatType.MoveSpeed:
            //    if (moveController != null)
            //    {
            //        moveController.AddMoveSpeed(amount);
            //    }
            //    break;

            //case BuffStatType.AttackSpeed:
            //    if (skillManager != null)
            //    {
            //        skillManager.AddAttackSpeed(amount);
            //    }
            //    break;

            //case BuffStatType.Defense:
            //    // 나중에 방어력 시스템을 만들면 여기서 위임
            //    // if (playerDefense != null) playerDefense.AddDefense(amount);
            //    break;
        }
    }
}
