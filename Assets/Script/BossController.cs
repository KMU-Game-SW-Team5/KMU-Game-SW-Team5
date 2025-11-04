using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossController : MonoBehaviour
{
    public float detectionRange = 15f;
    public float attackRange = 5f;
    public float moveSpeed = 2f;
    public int maxHealth = 1000;
    private int currentHealth;

    public int attackDamage = 10;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    public float deathAnimationDuration = 3f;
    private bool isDead = false;
    
    private Transform player;
    private Player playerScript;
    private Rigidbody rb;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("보스: 자식 오브젝트에서 Animator 컴포넌트를 찾을 수 없습니다!");
        }

        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (isDead) return; 

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
                animator.SetFloat("Speed", 1f); 
            }
            else if (distance <= attackRange)
            {
                AttackPlayer();
                animator.SetFloat("Speed", 0f);
            }
            else
            {
                animator.SetFloat("Speed", 0f);
            }
        }
        else
        {
            if(animator != null)
                animator.SetFloat("Speed", 0f);
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

                animator.SetTrigger("Attack");

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
        if (isDead) return; 

        currentHealth -= damage;
        Debug.Log("보스 체력: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("보스가 쓰러졌습니다.");
        
        animator.SetTrigger("Die"); 

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, deathAnimationDuration);
    }
}