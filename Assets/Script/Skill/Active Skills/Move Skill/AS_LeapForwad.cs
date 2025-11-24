using System.Collections;
using UnityEngine;


[CreateAssetMenu(fileName = "AS_LeapForward", menuName = "Scriptable Object/Active Skill/MoveType/LeapForward")]
public class AS_LeapForward : ActiveSkillBase
{
    [Header("도약 설정")]
    [SerializeField] private float horizontalSpeed = 10f; // 전방으로 얼마나 빠르게
    [SerializeField] private float upwardSpeed = 10f;     // 얼마나 높이 뜨는지
    [SerializeField] private float leapDuration = 0.6f;   // 도약 유지 시간

    protected override void Execute(GameObject user, Transform target)
    {
        if (user == null) return;

        var mover = user.GetComponent<MoveController>();
        if (mover == null) return;

        // 카메라 전방 기준으로 도약 (수평 방향만 사용)
        Vector3 dir;
        if (SkillManager.cam != null)
            dir = SkillManager.cam.transform.forward;
        else
            dir = user.transform.forward;

        // 도약 시작 요청
        mover.StartSkillMove(
            worldDirection: dir,
            horizontalSpeed: horizontalSpeed,
            upwardSpeed: upwardSpeed,
            duration: leapDuration,
            useGravity: true
        );
    }
}
