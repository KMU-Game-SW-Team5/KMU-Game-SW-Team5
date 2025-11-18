using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Boss3Controller : MonoBehaviour, IDamageable
{
    private float detectionRange = 300f;
    private float attackRange = 70f;
    public float moveSpeed = 30f;
    public int maxHealth = 1000;
    private int currentHealth;

    private int attackDamage = 10;
    private float attackCooldown = 2f;
    private float lastAttackTime;

    private float deathAnimationDuration = 3f;
    private bool isDead = false;
    
    
    private bool isPhase2 = false; 

    private Transform player;
    private Player playerScript;
    private Rigidbody rb;

    private Animator animator;


    public GameObject bulletPrefab; 
    public Transform muzzleTransform; 
    
    
    public float attack2Delay = 0.5f; 
    public float spreadAngle = 30f;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        rb.useGravity = true;
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponentInChildren<Animator>();
        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }
    
    void Update()
    {
        
        if (!isPhase2 && currentHealth <= maxHealth / 2)
        {
            isPhase2 = true;
            
            
            attackRange = 300f;
            detectionRange = 500f;
            attackDamage = 50; 
            attackCooldown = 0.5f;
            Debug.Log("보스2페이즈");
        }
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
                if (currentHealth > maxHealth / 2)
                {
                    animator.SetTrigger("Attack1");
                    playerScript.TakeDamage(attackDamage);
                }
                else
                {
                    StartCoroutine(Attack2Routine());
                }
                lastAttackTime = Time.time;
            }
        }
    }

    // Attack2 지연 발사 코루틴
    IEnumerator Attack2Routine()
    {
        animator.SetTrigger("Attack2");
        
        yield return new WaitForSeconds(attack2Delay);

        ShootBullet();
    }


    private void ShootBullet()
    {
        if (bulletPrefab == null || muzzleTransform == null)
        {
            return;
        }

        if (player == null)
        {
            return;
        }

        Vector3 targetPosition = player.position + Vector3.up * 1.0f;
        Vector3 directionToPlayer = (targetPosition - muzzleTransform.position).normalized;
        Quaternion centerRotation = Quaternion.LookRotation(directionToPlayer);

        float halfSpread = spreadAngle / 2.0f;
        
        Quaternion leftRotation = centerRotation * Quaternion.Euler(0, -halfSpread, 0);
        Quaternion rightRotation = centerRotation * Quaternion.Euler(0, halfSpread, 0);
        
        
        InstantiateBullet(centerRotation); // 중앙 총알
        InstantiateBullet(leftRotation);   // 왼쪽 총알
        InstantiateBullet(rightRotation);  // 오른쪽 총알
    }
    private void InstantiateBullet(Quaternion rotation)
    {
        GameObject bulletGO = Instantiate(bulletPrefab, muzzleTransform.position, rotation);
        
        Bullet projectile = bulletGO.GetComponent<Bullet>(); 
        
        if (projectile != null)
        {
            projectile.Damage = attackDamage; 
        }
    }

    
    public void TakeDamage(int damage)
    {
        if (isDead) return; 

        currentHealth -= damage;
        Debug.Log("보스 3 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("보스 3 사망");
        
        animator.SetTrigger("Die"); 

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, deathAnimationDuration);
    }
}