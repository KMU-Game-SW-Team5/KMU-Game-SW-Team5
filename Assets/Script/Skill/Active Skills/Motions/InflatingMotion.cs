using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Motion/Inflating")]
public class InflatingMotion : Motion
{
    [Header("Inflating Motion Settings")]
    [Tooltip("시작 로컬 스케일")]
    [SerializeField] private Vector3 startScale = Vector3.one;
    [Tooltip("최대 로컬 스케일")]
    [SerializeField] private Vector3 maxScale = Vector3.one * 2f;
    [Tooltip("시작 크기에서 최대 크기까지 걸리는 시간(초). 0 이하면 즉시 최대 크기 적용")]
    [SerializeField] private float inflateDuration = 0.5f;

    // 런타임 전용 상태
    private float elapsedTime = 0f;

    public override void SetVariables(Transform user, Transform target, Vector3 startVelocity, float motionSpeed = 1f)
    {
        // 입력값 그대로 수용
        this.user = user;
        this.target = target;
        this.velocity = startVelocity;
        this.motionSpeed = motionSpeed;

        // 런타임 카운터 초기화
        elapsedTime = 0f;

        // 초기 스케일 즉시 적용 (안정성)
        if (this.user != null)
            this.user.localScale = startScale;
    }

    public override void Move()
    {
        // 기본 이동 로직 수행
        base.Move();

        if (user == null) return;

        // 시간 누적: Motion.Move에서 사용한 것과 동일한 시간 스케일을 적용
        float delta = Time.fixedDeltaTime * motionSpeed;

        if (inflateDuration <= 0f)
        {
            // 즉시 최대 크기
            user.localScale = maxScale;
            return;
        }

        elapsedTime += delta;
        float t = Mathf.Clamp01(elapsedTime / inflateDuration);
        user.localScale = Vector3.Lerp(startScale, maxScale, t);
    }
}