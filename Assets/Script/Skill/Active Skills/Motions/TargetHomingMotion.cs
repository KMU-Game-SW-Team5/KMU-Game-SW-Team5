using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Motion/TargetHoming")]
public class TargetHomingMotion : Motion
{
    [Header("초기 발사 방향 설정")]
    [Tooltip("진행 방향에 수직한 평면에서 휘는 방향과 세기 (가속도 벡터의 방향 기준만 사용)")]
    public Vector2 initVector = new Vector2(0f, -1f);

    [Range(0, 90)]
    [Tooltip("진행 방향 벡터에서 휘는 방향 벡터로 움직이는 각도 (발사 각도)")]
    public int angle = 45;

    [Header("난사 모드")]
    [Tooltip("체크 시 initVector 대신 랜덤 단위 벡터, angle은 0~angle 사이 랜덤 적용")]
    public bool isDispersing = false;

    [Header("유도탄 설정")]
    [Range(0f, 10f)] public float baseHomingPower = 2f;
    [Range(0f, 10f)] public float homingAccel = 1f;
    [Range(0f, 720f)] public float maxTurnRate = 180f;
    [Range(0f, 1f)] public float inertia = 0.8f;
    [Range(0, 100)] public int speedAccel = 1;
    [Range(0, 100)] public int maxSpeed = 1;
    public float explodeDistance = 0.5f;

    [Header("궤적 흔들기 설정")]
    [Range(0f, 1f)] public float jitterStrength = 0.2f;
    [Range(0.1f, 5f)] public float jitterFrequency = 1f;

    private float speed;
    private Vector3 direction;
    private float currentHomingPower;
    private float noiseOffset;

    public override void SetVariables(Transform user, Transform target, Vector3 startVelocity, float motionSpeed = 1f)
    {
        this.user = user;
        this.target = target;
        this.motionSpeed = motionSpeed;
        this.velocity = startVelocity;
        this.speed = startVelocity.magnitude;

        // === 초기 발사 방향 계산 ===
        direction = startVelocity.normalized;

        Vector3 right = -Vector3.Cross(direction, Vector3.up).normalized;
        if (right == Vector3.zero)
        {
            float rand = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            right = new Vector3(Mathf.Cos(rand), 0f, Mathf.Sin(rand));
        }

        Vector3 up = -Vector3.Cross(right, direction).normalized;
        Vector3 curveDir;

        if (isDispersing)
        {
            // 무작위 방향 분산
            curveDir = Random.onUnitSphere;
            angle = Random.Range(0, angle + 1); // 0 ~ angle 사이 무작위
        }
        else
        {
            curveDir = -(right * initVector.x + up * initVector.y);
        }

        Vector3 rotAxis = Vector3.Cross(direction, -curveDir).normalized;
        velocity = (Quaternion.AngleAxis(angle, rotAxis) * direction) * speed;

        currentHomingPower = baseHomingPower;
        noiseOffset = Random.Range(0f, 999f);
    }

    public override void Move()
    {
        if (user == null) return;

        if (target != null)
        {
            Vector3 toTarget = (target.position - user.position).normalized;

            // Perlin 노이즈 기반 방향 흔들림
            float t = Time.time * jitterFrequency + noiseOffset;
            Vector3 randomDir = new Vector3(
                Mathf.PerlinNoise(t, 0f) - 0.5f,
                Mathf.PerlinNoise(0f, t) - 0.5f,
                Mathf.PerlinNoise(t, t * 0.5f) - 0.5f
            ).normalized;
            toTarget = (toTarget + randomDir * jitterStrength).normalized;

            float angleToTarget = Vector3.Angle(velocity.normalized, toTarget);

            currentHomingPower += homingAccel * Time.fixedDeltaTime;
            float maxDelta = maxTurnRate * Mathf.Deg2Rad * Time.fixedDeltaTime * motionSpeed * currentHomingPower;

            if (angleToTarget < 3f)
                velocity = toTarget * speed;
            else
                velocity = Vector3.RotateTowards(velocity.normalized * speed, toTarget * speed, maxDelta, 0f);

            speed += speedAccel * Time.fixedDeltaTime;

            if (Vector3.Distance(user.position, target.position) < explodeDistance)
            {
                var pc = user.GetComponent<ProjectileComponent>();
                if (pc != null) pc.Bomb();
                return;
            }
        }
        else
        {
            // exponential decay: inertia는 "속도 유지율"에 가까운 개념
            float damping = Mathf.Lerp(5f, 0f, inertia);
            // inertia = 1 → damping=0 (유지)
            // inertia = 0 → damping=5 (빠른 감속)
            velocity *= Mathf.Exp(-damping * Time.fixedDeltaTime);
        }
        user.position += velocity * Time.fixedDeltaTime * motionSpeed;
    }
}
