using UnityEngine;

/// <summary>
/// 투사체의 운동 방식을 정의하는 추상 클래스.
/// Motion은 Strategy 패턴으로 ProjectileComponent에 주입된다.
/// </summary>
public abstract class Motion : ScriptableObject
{
    [HideInInspector] public Transform user;     // 운동하는 대상
    [HideInInspector] public Transform target;   // 목표 또는 기준 대상
    [HideInInspector] public Vector3 velocity;   // 현재 속도 벡터
    [HideInInspector] public float motionSpeed;  // 운동 재생 속도 (TimeScale 비슷한 역할)

    protected Vector3 acceleration;                // 내부에서 사용하는 가속도 벡터

    // 운동 계산에 필요한 변수를 초기화함. 내용은 로직마다 다르게 구현.
    public abstract void SetVariables(Transform user, Transform target, 
        Vector3 startVelocity, float motionSpeed = 1f);

    // FixedUpdate마다 호출되어 이동을 수행
    public virtual void Move()
    {
        if (user == null) return;
        user.position += velocity * Time.fixedDeltaTime * motionSpeed;
        velocity += acceleration * Time.fixedDeltaTime * motionSpeed;
    }
}
