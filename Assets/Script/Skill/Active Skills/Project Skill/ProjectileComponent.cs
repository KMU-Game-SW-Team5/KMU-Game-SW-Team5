using UnityEngine;
using System.Collections;

public class ProjectileComponent : MonoBehaviour
{
    private float baseDamage;        // 기본 데미지
    private float lifetime;          // 지속 시간
    private int penetrationCount;    // 관통 횟수 (0 = 비관통: 첫 몬스터 히트 시 소멸)

    private Motion motionType;       // 운동 로직

    [SerializeField] private GameObject ExplosionEffectPrefab;
    [SerializeField] private AudioClip ExplosionSound;

    private GameObject projectilePrefabRef; // 오브젝트 풀링 키
    private SkillManager skillManager;      // 싱글톤 SkillManager
    private TrailRenderer trailRenderer;


    // ---------------------------------------------------------------------
    // 초기화
    // ---------------------------------------------------------------------
    private void Awake()
    {
        // Projectile 레이어 설정
        gameObject.layer = LayerMask.NameToLayer("Projectile");

        if (ExplosionEffectPrefab != null)
            ExplosionEffectPrefab.layer = LayerMask.NameToLayer("Projectile");

        // 싱글톤 SkillManager 가져오기
        skillManager = SkillManager.Instance;

        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        ResetState();
    }

    private void ResetState()
    {
        if (trailRenderer != null)
            trailRenderer.Clear();
        lifetime = Mathf.Max(lifetime, 0f);

        // penetrationCount는 SetDestroyComponent에서 설정되므로 여기서는 아무 동작도 하지 않음.
        // (혹시 풀링에서 초기값을 보장하려면 기본값을 설정하려면 여기에 추가 가능)
    }


    // ---------------------------------------------------------------------
    // Skill에서 넘겨주는 데미지만 초기화
    // ---------------------------------------------------------------------
    public void Initialize(float baseDamage)
    {
        this.baseDamage = baseDamage;
    }


    // ---------------------------------------------------------------------
    // 파괴 관련 설정
    // penetrationCount: 몬스터에 닿을 때마다 1씩 감소. 감소 후 0이 되면 그 순간 소멸.
    // 0이면 비관통(첫 히트 시 소멸).
    // ---------------------------------------------------------------------
    public void SetDestroyComponent(float Lifetime, int penetrationCount)
    {
        this.lifetime = Lifetime;
        this.penetrationCount = Mathf.Max(0, penetrationCount);
    }


    // ---------------------------------------------------------------------
    // Motion 설정
    // ---------------------------------------------------------------------
    public void SetMotionType(Motion newMotionType)
    {
        if (newMotionType == null)
        {
            motionType = null;
            return;
        }

        motionType = ScriptableObject.Instantiate(newMotionType);
    }

    public void SetPhysicalComponent(Transform _target, Vector3 _velocity, float _motionSpeed)
    {
        motionType?.SetVariables(this.transform, _target, _velocity, _motionSpeed);
    }

    // ---------------------------------------------------------------------
    // 충돌 처리
    // ---------------------------------------------------------------------
    private void OnTriggerEnter(Collider other)
    {
        Transform root = other.transform.root;

        // 🔹 Tag 기반 판별: 일반 몬스터("Monster") + 보스("Boss") 모두 포함
        bool isMonsterTag =
            other.CompareTag("Monster") ||
            root.CompareTag("Monster") ||
            other.CompareTag("Boss") ||
            root.CompareTag("Boss");

        if (isMonsterTag)
        {
            // 🔹 MonsterBase 찾기 (자식 콜라이더 고려)
            if (other.TryGetComponent<MonsterBase>(out var monster) ||
                root.TryGetComponent<MonsterBase>(out monster))
            {
                GameObject attacker = skillManager.owner;

                // ① 기본 데미지 적용
                monster.TakeDamage(baseDamage, attacker);

                // ② HitContext 생성
                HitContext ctx = new HitContext(
                    attacker: attacker,
                    target: monster.gameObject,
                    hitPoint: transform.position,
                    baseDamage: baseDamage,
                    source: this
                );

                // ③ 적중시 효과 발동
                skillManager.OnHit(ctx);
            }

            Bomb();

            // 몬스터에 닿았을 때 관통 카운트 처리:
            // penetrationCount > 0 이면 1 감소. 감소 후 0이면 소멸.
            // penetrationCount == 0 이면 비관통: 즉시 소멸.
            if (penetrationCount > 0)
            {
                penetrationCount--;
                if (penetrationCount < 0)
                    DespawnProjectile();
            }
            else
            {
                DespawnProjectile();
            }
        }
        else
        {
            // 몬스터가 아닌 것(지형/벽 등)과 충돌하면 항상 폭발/소멸 처리
            Bomb();
        }
    }



    // ---------------------------------------------------------------------
    // 폭발 이펙트 발생
    // ---------------------------------------------------------------------
    public void Bomb()
    {
        if (ExplosionEffectPrefab != null)
        {
            GameObject effect = ObjectPooler.Instance.Spawn(
                ExplosionEffectPrefab,
                transform.position,
                Quaternion.identity
            );

            var ec = effect.GetComponent<ExplosionEffectComponent>();
            if (ec != null)
                ec.SetPrefabRef(ExplosionEffectPrefab);

            if (ExplosionSound != null)
            {
                // 폭발 사운드 재생 (폭발 방향의 정해진 거리에서 재생)
                Vector3 dir = (transform.position - Camera.main.transform.position).normalized;
                Vector3 playPos = Camera.main.transform.position + dir * 3f;  // 여기서 거리 설정
                AudioSource.PlayClipAtPoint(ExplosionSound, playPos);
            }

        }
        else
        {
            Debug.Log("Explosion effect is null");
        }
    }


    // ---------------------------------------------------------------------
    // Update 루프
    // ---------------------------------------------------------------------
    private void FixedUpdate()
    {
        ManageLifetime();
        Move();
    }

    private void ManageLifetime()
    {
        lifetime -= Time.fixedDeltaTime;
        if (lifetime <= 0f)
        {
            DespawnProjectile();
        }
    }

    private void Move()
    {
        motionType?.Move();
    }


    // ---------------------------------------------------------------------
    // 오브젝트 풀링 관련
    // ---------------------------------------------------------------------
    public void SetPrefabRef(GameObject prefab)
    {
        projectilePrefabRef = prefab;
    }

    public void DespawnProjectile()
    {
        ObjectPooler.Instance.Despawn(projectilePrefabRef, gameObject);
    }
}
