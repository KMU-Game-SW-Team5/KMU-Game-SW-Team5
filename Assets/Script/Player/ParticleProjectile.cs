using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(Collider))]
public class ParticleProjectile : MonoBehaviour
{
    public float speed = 5f;
    public int Damage = 20;

    private ParticleSystem ps;
    private Collider col;     
    private bool isHit = false;

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        col = GetComponent<Collider>();

        Destroy(gameObject, ps.main.startLifetime.constantMax);
    }

    void Update()
    {
        if (!isHit)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
    
        if (isHit) return;


        var root = other.transform.root;
        Debug.Log($"[Particle] Hit: {other.name} (Tag: {other.tag}), Root: {root.name} (RootTag: {root.tag})");

        bool targetHit = false;

        // 몬스터 충돌 체크
        if (other.CompareTag("Monster") || root.CompareTag("Monster"))
        {
            Debug.Log($"[Particle] Monster hit detected on {other.name}");
            EventManager.MonsterHit();
            targetHit = true;
        }
        // 보스 충돌 체크
        else if (other.CompareTag("Boss") || root.CompareTag("Boss"))
        {
            Debug.Log("[Particle] Boss hit detected, searching for BossController...");
            BossController boss = other.GetComponentInParent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(Damage);
                Debug.Log($"[Particle] Hit Boss! Damage applied to {boss.gameObject.name}");
            }
            else
            {
                Debug.LogError($"[Particle] BossController not found in parents of {other.name}.");
            }
            EventManager.MonsterHit();
            targetHit = true;
        }
        else
        {
            // 몬스터나 보스가 아닌 다른 것에 부딪혔을 때
    
        }


        HandleImpact();
    }

    /// <summary>
    /// 이펙트가 어딘가에 충돌했을 때 호출되는 함수
    /// </summary>
    private void HandleImpact()
    {
        // 1. 충돌 상태로 변경
        isHit = true;

        // 2. 더 이상 이동하지 않도록 속도 0 (Update문이 멈춤)
        speed = 0; 
        
        // 3. 더 이상 충돌 감지 안 함 (콜라이더 비활성화)
        // (이걸 안하면 파티클이 사라지는 동안 계속 OnTriggerEnter가 호출될 수 있음)
        col.enabled = false; 

        // 4. 파티클 시스템의 "새 파티클 생성"을 중지
        ps.Stop();

        // 5. 이미 생성된 파티클이 모두 사라질 때까지 기다린 후 오브젝트를 파괴
        // (ps.main.duration을 사용하거나, 적절히 1~2초 뒤에 파괴해도 됩니다)
        Destroy(gameObject, ps.main.duration);
    }
}