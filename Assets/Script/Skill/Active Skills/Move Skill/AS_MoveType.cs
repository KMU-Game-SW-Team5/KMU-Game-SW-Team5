using System.Collections;
using UnityEngine;

public abstract class AS_MoveType : ActiveSkillBase
{
    [Header("공통 이동 설정")]
    [SerializeField] protected float moveSpeed = 15f;      // 기본 이동 속도
    [SerializeField] protected float moveDuration = 0.5f;  // 기본 이동 지속시간
    [SerializeField] protected bool useCameraForward = true;   // 카메라 기준
    [SerializeField] protected bool useTargetDirection = false;// 타겟(anchor) 방향 사용 여부
    [SerializeField] protected bool flattenY = true;           // 수평으로만 이동할지

    protected override void Execute(GameObject user, Transform target)
    {
        if (user == null) return;

        Transform userT = user.transform;

        // 1) 이동 방향 결정
        Vector3 dir = GetMoveDirection(userT, target);
        if (dir.sqrMagnitude < 0.0001f) return;

        if (flattenY)
        {
            dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = userT.forward;
        }

        dir.Normalize();

        // 2) 실제 이동은 자식 클래스에 위임
        if (SkillManager.Instance != null)
            SkillManager.Instance.StartCoroutine(MoveRoutine(userT, dir));
    }

    // 기본 방향 계산 로직 (원하면 자식에서 override 할 수도 있음)
    protected virtual Vector3 GetMoveDirection(Transform user, Transform target)
    {
        if (useTargetDirection && target != null)
        {
            return (target.position - user.position);
        }

        if (useCameraForward && SkillManager.cam != null)
        {
            return SkillManager.cam.transform.forward;
        }

        return user.forward;
    }

    // 실제로 Transform을 어떻게 움직일지는 자식이 구현
    protected abstract IEnumerator MoveRoutine(Transform user, Vector3 dir);
}
