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
            Debug.Log("[Particle] Hit non-target object.");
    
        }


        HandleImpact();
    }

    /// 이펙트가 어딘가에 충돌했을 때 호출되는 함수
    private void HandleImpact()
    {
        isHit = true;

        speed = 0; 
        
        col.enabled = false; 

        ps.Stop();

        Destroy(gameObject, ps.main.duration);
    }
}