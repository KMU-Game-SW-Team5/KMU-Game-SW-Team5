using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AS_Dash", menuName = "Scriptable Object/Active Skill/MoveType/Dash")]
public class AS_Dash : ActiveSkillBase
{
    [Header("대시 설정")]
    [SerializeField] private float dashSpeed = 15f;   // 초당 이동 속도
    [SerializeField] private float dashDuration = 0.3f; // 대시 유지 시간(=무적 시간)

    protected override void Execute(GameObject user, Transform target)
    {
        if (user == null) return;

        // 이동 컨트롤러 찾기
        var mover = user.GetComponent<MoveController>();
        if (mover == null)
        {
            Debug.LogWarning("[AS_Dash] MoveController를 찾을 수 없습니다.");
            return;
        }

        // 1) 입력 기준 방향 (로컬 forward 기준 8방향)
        Vector3 inputDir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) inputDir += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) inputDir += Vector3.back;
        if (Input.GetKey(KeyCode.A)) inputDir += Vector3.left;
        if (Input.GetKey(KeyCode.D)) inputDir += Vector3.right;

        // 아무 키도 안 누르면 전방
        if (inputDir == Vector3.zero)
            inputDir = Vector3.forward;

        // 2) 카메라 전방
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0f;    
        camForward.Normalize();

        // 3) "Vector3.forward를 camForward로 돌려주는 회전" 만들기
        Quaternion camRot = Quaternion.FromToRotation(Vector3.forward, camForward);

        // 4) 입력 방향을 카메라 기준으로 회전
        Vector3 worldDir = camRot * inputDir;
        worldDir.y = 0f;
        worldDir.Normalize();


        // 5) MoveController에게 "대시 모드" 요청
        // 수평 이동만 하고, 중력은 적용 안 함 (돌진할 때 높이 그대로 유지)
        mover.StartSkillMove(
            worldDirection: worldDir,
            horizontalSpeed: dashSpeed,
            upwardSpeed: 0f,
            duration: dashDuration,
            useGravity: false
        );

        // 6) 대시 동안 무적 처리
        var player = user.GetComponent<Player>();
        if (player != null)
        {
            player.SetInvincibleFor(dashDuration);
        }
    }
}
