using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 5f; // 총알 속도

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
        
        var root = other.transform.root; // 최상위 부모 (RigidBody 있는 BossCapsule)
        Debug.Log($"[Bullet] Hit: {other.name} (Tag: {other.tag}), Root: {root.name} (RootTag: {root.tag})");

        
        if (other.CompareTag("Monster") || root.CompareTag("Monster"))
        {
            Debug.Log($"[Bullet] Monster hit detected on {other.name}");
            EventManager.MonsterHit();
            Destroy(gameObject);
            return;
        }

        
        if (other.CompareTag("Boss") || root.CompareTag("Boss"))
        {
            Debug.Log("[Bullet] Boss hit detected, searching for BossController...");

            // 부모 계층에서 BossController 탐색
            BossController boss = other.GetComponentInParent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(10); // 데미지 적용
                Debug.Log($"[Bullet] Hit Boss! Damage applied to {boss.gameObject.name}");
            }
            else
            {
                Debug.LogError($"[Bullet] BossController not found in parents of {other.name}. Check hierarchy!");
            }
            EventManager.MonsterHit();

            Destroy(gameObject);
            return;
        }

    }
}
