using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f; 
    public int Damage = 10;

    void Start()
    {
        // 5초 뒤 자동 삭제
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        // 전방으로 이동
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponentInParent<IDamageable>();

        if (target != null)
        {
            
            target.TakeDamage(Damage);
            
            
            
            
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
        
    }
}