using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss2Controller : MonoBehaviour, IDamageable
{
    public float detectionRange = 300f;
    [SerializeField] private float attackRange = 30f;
    public float moveSpeed = 30f;
    public int maxHealth = 1000;
    private int currentHealth;

    

    public int attackDamage = 10;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    private float deathAnimationDuration = 3f;
    private bool isDead = false;
    
    private Transform player;
    private Player playerScript;
    private Rigidbody rb;

    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        // (추가) Y축 고정 (바닥 뚫기 방지)
        rb.useGravity = true;
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponentInChildren<Animator>();

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
                if(animator != null) animator.SetFloat("Speed", 1f); 
            }
            else if (distance <= attackRange)
            {
                AttackPlayer();
                if(animator != null) animator.SetFloat("Speed", 0f);
            }
            else
            {
                if(animator != null) animator.SetFloat("Speed", 0f);
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
        
        Vector3 lookPosition = player.position;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);
        
        
        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;

        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    
    void AttackPlayer()
    {
    
        Vector3 lookPosition = player.position;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            if (playerScript != null)
            {
                Debug.Log("벌레 보스 공격"); 

                
                if (currentHealth > maxHealth / 2)
                {
                    
                    animator.SetTrigger("Attack1");
                    playerScript.TakeDamage(attackDamage);
                }
                else
                {
                    animator.SetTrigger("Attack2");
                    playerScript.TakeDamage(attackDamage * 2);
                    
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
        Debug.Log("벌레 보스 체력: " + currentHealth);

        

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("벌레 보스가 쓰러졌습니다.");
        
        animator.SetTrigger("Die"); 

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, deathAnimationDuration);
    }
}