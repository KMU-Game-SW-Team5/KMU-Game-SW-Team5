using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Boss3Controller : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private float detectionRange = 300f;
    [SerializeField] private float attackRange = 70f;
    public float moveSpeed = 30f;
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("Combat")]
    private int attackDamage = 10;
    private float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("State")]
    private float deathAnimationDuration = 3f;
    private bool isDead = false;
    private bool isPhase2 = false; 

    [Header("References")]
    private Transform player;
    private Player playerScript;
    private Rigidbody rb;
    private Animator animator;

    [Header("Ranged Attack")]
    public GameObject bulletPrefab; 
    public Transform muzzleTransform; 
    public float attack2Delay = 100f; 
    public float spreadAngle = 3f;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips; 
    
    
    private float lastProgress = 0f; 
    [SerializeField] private AudioClip deathClip;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        
        rb.useGravity = true;
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponentInChildren<Animator>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();

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
            attackCooldown = 2f;
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

                // [핵심] 2발자국 소리 체크
                CheckDualFootsteps();
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

    // [핵심 수정] 1번 루프에 2번 소리내기 (0% 지점, 50% 지점)
    void CheckDualFootsteps()
    {
        if (animator == null) return;

        // 현재 애니메이션의 진행도 (0.0 ~ 1.0 사이의 값으로 변환)
        float currentProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;

        // 1. 루프가 다시 시작될 때 (0.9 -> 0.1 로 넘어가는 순간) -> 첫 번째 발
        if (currentProgress < lastProgress)
        {
            PlayWalkSound();
        }
        // 2. 중간 지점을 통과할 때 (0.4 -> 0.6 으로 넘어가는 순간) -> 두 번째 발
        else if (lastProgress < 0.5f && currentProgress >= 0.5f)
        {
            PlayWalkSound();
        }

        // 현재 진행도를 저장해서 다음 프레임에 비교
        lastProgress = currentProgress;
    }

    void PlayWalkSound()
    {
        if (audioSource == null || walkClips == null || walkClips.Length == 0) return;

        int index = Random.Range(0, walkClips.Length);
        if (walkClips[index] != null)
        {
            // 양발이므로 피치를 조금 더 다양하게 줘서 자연스럽게
            audioSource.pitch = Random.Range(0.85f, 1.0f); 
            audioSource.PlayOneShot(walkClips[index]);
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
        if (player == null) return; 

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

    IEnumerator Attack2Routine()
    {
        animator.SetTrigger("Attack2");
        yield return new WaitForSeconds(attack2Delay);
        ShootBullet();
    }

    private void ShootBullet()
    {
        if (bulletPrefab == null || muzzleTransform == null || player == null) return;

        Vector3 targetPosition = player.position + Vector3.up * 1.0f;
        Vector3 directionToPlayer = (targetPosition - muzzleTransform.position).normalized;
        Quaternion centerRotation = Quaternion.LookRotation(directionToPlayer);

        float halfSpread = spreadAngle / 2.0f;
        
        Quaternion leftRotation = centerRotation * Quaternion.Euler(0, -halfSpread, 0);
        Quaternion rightRotation = centerRotation * Quaternion.Euler(0, halfSpread, 0);
        
        InstantiateBullet(centerRotation); 
        InstantiateBullet(leftRotation);   
        InstantiateBullet(rightRotation);  
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
        if (audioSource != null && deathClip != null)
        {
            
            audioSource.pitch = 1.0f; 
            audioSource.PlayOneShot(deathClip);
        }
        animator.SetTrigger("Die"); 

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, deathAnimationDuration);
    }
}