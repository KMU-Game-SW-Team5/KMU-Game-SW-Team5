using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossController : MonoBehaviour
{
    public float detectionRange;
    public float attackRange;
    public float moveSpeed;
    public int maxHealth;
    private int currentHealth;

    public int attackDamage;
    public float attackCooldown;
    private float lastAttackTime;
    
    private Transform player;
    private Player playerScript;
    private Rigidbody rb;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
            playerScript = null;
        }
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= detectionRange && distance > attackRange)
            {
                MoveTowardsPlayer();
            }
            else if (distance <= attackRange)
            {
                AttackPlayer();
            }
        }
    }

    void FindPlayer()
    {
        if (player == null || !player.gameObject.activeInHierarchy)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
                playerScript = playerObject.GetComponent<Player>();
                
                if (playerScript == null)
                {
                    Debug.LogError("보스: 'Player' 태그를 가진 오브젝트를 찾았지만, Player.cs 스크립트가 없습니다!");
                }
            }
        }
    }

    void MoveTowardsPlayer()
    {
        transform.LookAt(player);
        Vector3 targetPosition = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(targetPosition);
    }

    void AttackPlayer()
    {
        transform.LookAt(player);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            if (playerScript != null)
            {
                Debug.Log("보스 공격!"); 

                playerScript.TakeDamage(attackDamage);

                lastAttackTime = Time.time;
            }
            else
            {
                Debug.LogWarning("보스: 공격하려 했으나 playerScript 참조가 null입니다.");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("보스 체력: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("보스가 쓰러졌습니다.");
        Destroy(gameObject);
    }
}