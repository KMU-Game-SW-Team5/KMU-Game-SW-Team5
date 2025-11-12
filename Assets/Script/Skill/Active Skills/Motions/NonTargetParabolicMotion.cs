using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Motion/NontargetParabolic")]
public class NontargetParabolicMotion : Motion
{
    [Header("포물선 운동 설정")]
    [Tooltip("진행 방향에 수직한 평면에서 휘는 방향과 세기 (가속도 벡터와 동일)")]
    public Vector2 curVector = new Vector2(0f, -9.81f);
    [Header("중력 여부")]
    [Tooltip("중력에 의한 포물선이라면 체크. y값만 사용됨.")]
    public bool isGravity = true;

    [Range(0, 90)]
    [Tooltip("진행 방향 벡터에서 휘는 방향 벡터로 움직이는 각도 (발사 각도)")]
    public int angle = 45;

    // 내부 계산용
    private float speed;         // 초기 속력의 크기
    private Vector3 direction;   // 진행 방향 (가속도는 이 방향에 수직한 평면에 존재)

    public override void SetVariables(Transform user, Transform target, Vector3 startVelocity, float motionSpeed = 1f)
    {
        // 기본 필드
        this.user = user;
        this.target = target;
        this.velocity = startVelocity;
        this.motionSpeed = motionSpeed;

        // 진행 방향 : 타깃을 이용하지 않고, 초기 속도 설정을 방향으로 이용함.
        direction = startVelocity.normalized;

        // 등속 직선 운동의 경우 즉시 종료
        if (curVector == Vector2.zero)
        {
            if (target != null) Object.Destroy(target.gameObject);
            return;
        }

        speed = startVelocity.magnitude;

        // 가속도 설정
        if (!isGravity)
        {
            // 진행 방향에 수직인 평면을 기준으로 가속도 방향 설정 (x증가 방향 = 오른쪽, y증가 방향 = 위)
            Vector3 right = -Vector3.Cross(direction, Vector3.up).normalized;
            if (right == Vector3.zero)      // 만약 진행 방향이 수직이라면 무작위 방향으로 설정
            {
                float rand = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                right = new Vector3(Mathf.Cos(rand), 0f, Mathf.Sin(rand));
            }
            Vector3 up = -Vector3.Cross(right, direction).normalized;
            acceleration = (right * curVector.x + up * curVector.y);
        }
        else    // 중력일 때는 y값만 사용
            acceleration = Vector3.up * curVector.y;


        // 발사 각도를 적용한 속도 (진행 방향에서 진행 방향과 가속도에 수직인 축을 기준으로 회전한 방향 * 속력)
        velocity = (Quaternion.AngleAxis(angle, Vector3.Cross(direction, -acceleration)) * direction)
            .normalized * speed;

    }

}
