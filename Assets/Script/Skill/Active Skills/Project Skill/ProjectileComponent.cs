using UnityEngine;

// 투사체의 데미지, 수명, 운동을 관리하는 컴포넌트.
public class ProjectileComponent : MonoBehaviour
{
    private float damage;       // 데미지
    private float lifetime;     // 지속 시간
    private bool penetrable;    // 관통 여부

    private Motion motionType;     // 운동 로직 (ScriptableObject 복제본)
    [SerializeField] public float motionSpeed = 1.0f;  // 모션 재생 속도 (가속 배율 등)

    [SerializeField] private GameObject ExplosionEffectPrefab; // 폭발 이펙트 프리팹

    // 소멸과 관련된 설정 (수명, 관통성)
    public void SetDestroyComponent(float Lifetime, bool Penetrable)
    {
        this.lifetime = Lifetime;
        this.penetrable = Penetrable;
    }

    // 운동 로직 설정 (인스턴스를 만들어야 값 공유를 방지할 수 있음)
    public void SetMotionType(Motion newMotionType)
    {   
        if (newMotionType == null)
        {
            motionType = null;
            return;
        }
        motionType = ScriptableObject.Instantiate(newMotionType);
    }


    // 운동 변수 설정
    public void SetPhysicalComponent(Transform _target, Vector3 _velocity)
    {
        motionType.SetVariables(this.transform, _target, _velocity, motionSpeed);
    }

    // 뭔가에 닿았을 때
    private void OnTriggerEnter(Collider other)
    {
        // TODO: 적 태그 검사 후 데미지 적용 추가 예정

        // 폭발 이펙트 생성
        if (ExplosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(ExplosionEffectPrefab, transform.position, Quaternion.identity);

            // 파티클 지속시간만큼 삭제 예약
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            Destroy(effect, ps ? ps.main.duration + ps.main.startLifetime.constantMax : 2f);
        }

        // 관통 불가면 자기 자신 삭제
        if (!penetrable)
            Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        ManageLifetime();
        Move();
    }

    // 수명 관리
    private void ManageLifetime()
    {
        lifetime -= Time.fixedDeltaTime;
        if (lifetime <= 0f)
            Destroy(gameObject);
    }

    // Motion 기반 이동
    private void Move()
    {
        motionType.Move();
    }
}
