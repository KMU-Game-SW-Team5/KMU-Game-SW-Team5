using System.Collections;
using UnityEngine;

public class BuffApplier : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private MoveController moveController;

    private void Awake()
    { 
        if (moveController == null)
            moveController = GetComponent<MoveController>();
    }

    // 입력 시간 동안 적용
    public void ApplyBuffFor(BuffStatType statType, float amount, float duration)
    {
        StartCoroutine(BuffRoutine(statType, amount, duration));
    }

    // 영구 적용
    public void ApplyBuff(BuffStatType type, float amount)
    {
        AddBuff(type, amount);
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

            case BuffStatType.Heal:
                SkillManager.Instance.player.Heal((int)amount);
                break;

            case BuffStatType.AttackSpeed:
                SkillManager.Instance.AddAttackSpeed(amount);
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
