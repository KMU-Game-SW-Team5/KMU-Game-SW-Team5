using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // Rigidbody를 강제로 요구합니다.
public class MonsterController : MonoBehaviour
{
    
    public float detectionRange = 100f;
    public float attackRange = 10f;    
    public float moveSpeed = 10f;    
    public int maxHealth = 1000;
    public int attackDamage = 5;      
    public float attackCooldown = 1f; 
    private float deathAnimationDuration = 3f;

    
    private int currentHealth;
    private float lastAttackTime;
    private bool isDead = false;

    private Transform player;
    private Player playerScript;
    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; 

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("몬스터: 자식 오브젝트에서 Animator 컴포넌트가 없음");
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
            if(animator != null) animator.SetFloat("Speed", 0f); // 멈춤
        }

        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= detectionRange && distance > attackRange)
            {
                rb.isKinematic = false;
                MoveTowardsPlayer();
                if(animator != null) animator.SetFloat("Speed", 1f); // 걷기/뛰기 애니메이션
            }
            else if (distance <= attackRange)
            {
                rb.isKinematic = true;
                AttackPlayer();
                if(animator != null) animator.SetFloat("Speed", 0f);
                
             
            }
            else
            {
                rb.isKinematic = true;
                if(animator != null) animator.SetFloat("Speed", 0f);
            }
        }
        else
        {

            if (animator != null) animator.SetFloat("Speed", 0f);
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
                    Debug.LogError("몬스터: 'Player' 태그 오브젝트에서 Player.cs 없음");
                }
            }
        }
    }

    void MoveTowardsPlayer()
    {
        
        Vector3 lookPosition = player.position;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);
        
        Vector3 targetPosition = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(targetPosition);
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
                Debug.Log("몬스터 공격");

                if(animator != null) animator.SetTrigger("Attack"); 
                
                playerScript.TakeDamage(attackDamage);

                lastAttackTime = Time.time;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log("몬스터 체력: " + currentHealth);


        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("몬스터가 쓰러졌습니다.");

        
        if(animator != null) animator.SetTrigger("Die");

        
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false; 

        
        Destroy(gameObject, deathAnimationDuration);
    }
}