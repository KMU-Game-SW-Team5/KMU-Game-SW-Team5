using UnityEngine;
using System.Collections;

// 투사체의 데미지, 수명, 운동을 관리하는 컴포넌트.
public class ProjectileComponent : MonoBehaviour
{
    private float damage;       // 데미지
    private float lifetime;     // 지속 시간
    private bool penetrable;    // 관통 여부

    private Motion motionType;     // 운동 로직 (ScriptableObject 복제본)

    [SerializeField] private GameObject ExplosionEffectPrefab; // 폭발 이펙트 프리팹

    [SerializeField] private GameObject projectilePrefabRef; // 오브젝트 풀러에서 키로 사용되는 정보


    private void Awake()
    {
        // 자신을 Projectile 레이어로 설정
        gameObject.layer = LayerMask.NameToLayer("Projectile");

        // 폭발 이펙트 프리팹도 Projectile 레이어로 맞춤
        if (ExplosionEffectPrefab != null)
            ExplosionEffectPrefab.layer = LayerMask.NameToLayer("Projectile");
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void ResetState()
    {
        // 남은 속도나 타겟 데이터가 꼬이지 않도록 초기화
        lifetime = Mathf.Max(lifetime, 0f);
    }


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
    public void SetPhysicalComponent(Transform _target, Vector3 _velocity, float _motionSpeed)
    {
        motionType.SetVariables(this.transform, _target, _velocity, _motionSpeed);
    }

    // 뭔가에 닿았을 때
    private void OnTriggerEnter(Collider other)
    {
        //// --- 🔍 충돌 로그 출력 ---
        //string otherName = other.gameObject.name;
        //string otherTag = other.gameObject.tag;
        //string otherLayer = LayerMask.LayerToName(other.gameObject.layer);

        //Debug.Log($"[ProjectileComponent] 충돌 발생 → 대상: {otherName}, 태그: {otherTag}, 레이어: {otherLayer}", other.gameObject);
        Bomb();
    }

    public void Bomb()
    {
        // --- 폭발 이펙트 생성 ---
        if (ExplosionEffectPrefab != null)
        {
            GameObject effect = ObjectPooler.Instance.Spawn(ExplosionEffectPrefab, transform.position, Quaternion.identity);

            // 🔹 prefab 참조 전달
            var ec = effect.GetComponent<ExplosionEffectComponent>();
            if (ec != null)
                ec.SetPrefabRef(ExplosionEffectPrefab);
        }

        // --- 관통 불가 시 비활성화 ---
        if (!penetrable)
        {
            DespawnProjectile();
        }
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
            DespawnProjectile();
    }

    // Motion 기반 이동
    private void Move()
    {
        motionType.Move();
    }

    // 오브젝트 풀러에서 쓸 키 설정
    public void SetPrefabRef(GameObject prefab)
    {
        projectilePrefabRef = prefab;
    }

    public void DespawnProjectile()
    {
        ObjectPooler.Instance.Despawn(projectilePrefabRef, gameObject);

    }

}
