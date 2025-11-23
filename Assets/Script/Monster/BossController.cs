using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossController : MonoBehaviour
{
    public float detectionRange = 300f;
    private float attackRange = 50f;
    public float moveSpeed = 2f;
    public int maxHealth = 1000;
    private int currentHealth;

    private bool hasEnteredPhase2 = false;

    public int attackDamage = 10;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    private float deathAnimationDuration = 3f;
    private bool isDead = false;
    
    private Transform player;
    private Player playerScript;
    private Rigidbody rb;

    private Animator animator;

    // [UI 연결] HP 변화 이벤트 
    private bool isPlayerDetected = false;
    public event Action<int, int> OnHPChanged;
    public event Action<int, int> OnAppeared;
    public event Action OnDisappeared;

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
                HandlePlayerDetected();
                animator.SetFloat("Speed", 1f); 
            }
            else if (distance <= attackRange)
            {
                AttackPlayer();
                animator.SetFloat("Speed", 0f);
            }
            else
            {
                HandlePlayerLost();
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
                    Debug.LogError("보스: 'Player' 태그를 가진 오브젝트를 찾았지만, Player.cs 없음");
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
                Debug.Log("보스 공격"); 
                if(currentHealth > 800)
                {
                    animator.SetTrigger("BasicAttack");
                    playerScript.TakeDamage(attackDamage);
                }
                else if(currentHealth > 600) 
                {
                    animator.SetTrigger("ClawAttack");
                    playerScript.TakeDamage(attackDamage*2);
                }
                else if(currentHealth > 500)
                {
                    animator.SetTrigger("FlameAttack");
                    playerScript.TakeDamage(attackDamage*3);
                }
                else
                {
                    animator.SetTrigger("FlyAttack");
                    playerScript.TakeDamage(attackDamage*4);
                }



                lastAttackTime = Time.time;
            }
            else
            {
                Debug.LogWarning("보스: 공격하려 했으나 playerScript 없음");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; 

        currentHealth -= damage;
        Debug.Log("보스 체력: " + currentHealth);

        // UI 연결
        OnHPChanged?.Invoke(currentHealth, maxHealth);

        if (!hasEnteredPhase2 && currentHealth <= 500 && currentHealth > 0)
        {
            attackRange = 100f;
            hasEnteredPhase2 = true;
            animator.SetBool("isPhase2", true);
            animator.SetTrigger("startPhase2"); // 2페이즈 즉시 돌입 트리거 발동
            Debug.Log("보스: 2페이즈");
        }


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

        // UI 연결
        HandlePlayerLost();

        animator.SetTrigger("Die"); 

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        // UI 관련 이벤트 함수 제거를 위한 보스 사라짐 알림
        BossManager.Instance.UnregisterBoss(this);

        Destroy(gameObject, deathAnimationDuration);
    }

    // UI 연결
    void HandlePlayerDetected()
    {
        if (isPlayerDetected)
        {
            // 이미 감지 된 경우
            return;
        }
        else
        {
            // 새롭게 감지된 경우
            OnAppeared?.Invoke(currentHealth, maxHealth);
            isPlayerDetected = true;
        }
    }

    // UI 연결
    void HandlePlayerLost()
    {
        isPlayerDetected = false;
        OnDisappeared?.Invoke();
    }
}