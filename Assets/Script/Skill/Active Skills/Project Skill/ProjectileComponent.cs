using UnityEngine;

// 투사체의 데미지와 충돌을 처리하는 컴포넌트임.

public class ProjectileComponent : MonoBehaviour
{
    private float damage;       // 데미지
    private float lifetime;     // 지속 시간
    private bool penetrable;    // 관통 여부

    private Rigidbody rb;
    public Vector3 velocity = Vector3.zero;   // 속도 벡터
    [SerializeField] public float speed = 1.0f;       // 속력 계수
    public Vector3 acceleration = Vector3.zero;   // 가속도 벡터
    private Transform target;      // 운동의 기준이 되는 점 (운동 방식에 따라 다르게 이용됨.)
    private Motion motionType;     // 운동 방식, 운동 알고리즘을 위임함.

    [SerializeField] private GameObject ExplosionEffectPrefab;    // 소멸(폭발) 이펙트 프리팹

    // 투사체 소멸 변수 설정(수명, 관통성)
    public void SetDestroyComponent(float Lifetime, bool Penetrable)
    {
        this.lifetime = Lifetime;
        this.penetrable = Penetrable;
    }

    // 투사체의 운동 방식 설정
    public void SetMotionType(Motion newMotionType)
    {
        this.motionType = newMotionType;
    }
    // 투사체 물리 변수 설정(속도벡터, 가속도벡터)
    public void SetPhysicalComponent(Vector3 Velocity, Vector3 Acceleration, Transform target)
    {
        this.velocity = Velocity;
        this.acceleration = Acceleration;
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO : 적인지 확인하여 데미지 적용하는 부분 추가할 것

        // 이펙트 생성 및 투사체 삭제(관통성이 없을 때만)
        if (ExplosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(ExplosionEffectPrefab, transform.position, Quaternion.identity);

            // 파티클 지속시간 후에 삭제되게 설정, 파티클을 안 쓰는 경우엔 2초 후 삭제
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(effect, ps.main.duration + ps.main.startLifetime.constantMax);
            else
                Destroy(effect, 2f); 
        }

        if (!penetrable) Destroy(gameObject);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Projectile"), LayerMask.NameToLayer("Projectile"));
    }

    // 투사체 운동 및 수명 반영
    private void FixedUpdate()
    {
        ManageLifetime();
        Move();
    }

    // 수명 관리
    private void ManageLifetime()
    {
        lifetime -= Time.fixedDeltaTime;
        if (lifetime < 0) Destroy(gameObject);
    }

    // 운동 알고리즘은 전략패턴으로 Motion에 위임함.
    private void Move()
    {
        if (motionType == null)
        {
            Debug.Log("운동 방식을 결정해야 함." + this.gameObject.ToString());
            return;
        }
        rb.velocity = velocity = motionType.GetNextVelocity(target, velocity, acceleration);
        //Debug.Log("속도 : " + rb.velocity);
    }

}
