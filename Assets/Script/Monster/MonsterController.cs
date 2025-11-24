using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))] 
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
        
        // (수정) Rigidbody의 중력을 사용하고, 회전은 스크립트로 제어하도록 X, Z축 고정
        rb.useGravity = true;
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("몬스터: 자식 오브젝트에서 Animator 컴포넌트가 없음");
        }

        
        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    // (수정) Update가 아닌 FixedUpdate에서 물리 처리
    void FixedUpdate()
    {
        if (isDead) return;

        
        if (player != null && !player.gameObject.activeInHierarchy)
        {
            
            player = null;
            playerScript = null;
            if(animator != null) animator.SetFloat("Speed", 0f); 
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
        // (수정) Y축(높이)을 무시하는 로직
        
        // 1. 바라볼 위치의 Y축을 몬스터 자신의 Y축으로 고정
        Vector3 lookPosition = player.position;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);
        
        // 2. 이동할 타겟 위치의 Y축도 몬스터 자신의 Y축으로 고정
        Vector3 targetPosition = player.position;
        targetPosition.y = transform.position.y;

        // 3. Y축이 고정된 타겟으로 이동
        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    void AttackPlayer()
    {
        // (수정) 공격 시에도 Y축(높이)을 무시하고 바라봄
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

        
        rb.isKinematic = true; // 사망 시 물리 정지
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        // Kill Counter 반영
        KillCounter.Instance.AddMonsterKill();

        Destroy(gameObject, deathAnimationDuration);
    }
}