using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f; // 총알 속도
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
        
        var root = other.transform.root;
        

        
        if (other.CompareTag("Monster") || root.CompareTag("Monster"))
        {
            
            
            MonsterController monster = other.GetComponentInParent<MonsterController>();
            if (monster != null)
            {
                
                monster.TakeDamage(Damage); 
                
            }
            else
            {
                
                
            }

            EventManager.MonsterHit();
            Destroy(gameObject);
            return;
        }

        
        if (other.CompareTag("Boss") || root.CompareTag("Boss"))
        {

         
            BossController boss = other.GetComponentInParent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(Damage); // 데미지 적용
         
            }
            else
            {
         
            }
            EventManager.MonsterHit();

            Destroy(gameObject);
            return;
        }

    }
}
