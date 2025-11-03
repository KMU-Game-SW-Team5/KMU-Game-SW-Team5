using UnityEngine;

// 투사체의 데미지와 충돌을 처리하는 컴포넌트임.

public class ProjectileComponent : MonoBehaviour
{
    private float damage;       // 데미지
    private float lifetime;     // 지속 시간
    private bool penetrable;    // 관통 여부

    private Rigidbody rb;
    private Vector3 velocity;   // 속도 벡터
    private float verticalAccel;   // 중력에 의한 가속도 (양수 = 부상, 0 = 직선, 음수 = 추락)

    [SerializeField] private GameObject hitEffectPrefab;    // 타격 이펙트 프리팹

    // 투사체 변수 설정
    public void SetComponent(float Damage, float Lifetime, bool Penetrable, Vector3 Velocity, float VerticalAccel)
    {
        this.damage = Damage;
        this.lifetime = Lifetime;
        this.velocity = Velocity;
        this.penetrable = Penetrable;
        this.verticalAccel = VerticalAccel;
        if (rb ==  null) rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // TODO : 적인지 확인하는 부분 추가할 것

        // 이펙트 생성 및 투사체 삭제(관통성이 없을 때만)
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);

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
        velocity.y += verticalAccel * Time.fixedDeltaTime;
        rb.velocity = velocity;
        lifetime -= Time.fixedDeltaTime;
        if (lifetime < 0) Destroy(gameObject);
    }

}
