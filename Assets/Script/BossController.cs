using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossController : MonoBehaviour
{
    public float detectionRange = 15f; // 플레이어를 감지할 수 있는 최대 범위
    public float attackRange = 5f;    // 보스가 멈출 거리 (공격 거리)
    public float moveSpeed = 2f;
    public int maxHealth = 1000;
    private int currentHealth;

    private Transform player;
    private Rigidbody rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            // 플레이어와의 거리를 계산
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= detectionRange && distance > attackRange)
            {
                MoveTowardsPlayer();
            }
           
        }
    }

    void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    void MoveTowardsPlayer()
    {
        transform.LookAt(player);
        Vector3 targetPosition = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(targetPosition);
    }

    // 데미지를 받는 함수
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("보스 체력: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 보스 사망 처리 함수
    void Die()
    {
        Debug.Log("보스가 쓰러졌습니다.");
        Destroy(gameObject);
    }
}