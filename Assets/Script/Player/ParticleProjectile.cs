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

        // "데미지를 받을 수 있는" 컴포넌트 (몬스터/보스1/보스2 등 모두)
        IDamageable damageable = root.GetComponent<IDamageable>();

        if (damageable != null)
        {
            
            damageable.TakeDamage(Damage);
            EventManager.MonsterHit();
            
            
            if (root.CompareTag("Monster"))
            {
                Destroy(gameObject);
                return;
            }
            
            if (root.CompareTag("Boss"))
            {
                
            }
        }
        else
        {
            
            Debug.Log("[Particle] Hit non-target object.");
        }

        
        HandleImpact();
    }
    private void HandleImpact()
    {
        isHit = true;
        speed = 0; 
        col.enabled = false; 
        ps.Stop();
        Destroy(gameObject, ps.main.duration);
    }
}