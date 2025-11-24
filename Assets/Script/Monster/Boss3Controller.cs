using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    [Header("Attack Settings")]
    public GameObject bulletPrefab; 
    public Transform muzzleTransform; 
    public float spreadAngle = 3f;
    
    
    public float attack1Delay = 0.5f;
    public float attack2Delay = 0.5f; 

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips; 
    [SerializeField] private AudioClip deathClip;

    [SerializeField] private AudioClip attack1Clip; 
    [SerializeField] private AudioClip attack2Clip;

    // [UI 연결] HP 변화 이벤트 
    private bool isPlayerDetected = false;
    public event Action<int, int> OnHPChanged;
    public event Action<int, int> OnAppeared;
    public event Action OnDisappeared;  

    private float lastProgress = 0f; 

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

    void CheckDualFootsteps()
    {
        if (animator == null) return;

        float currentProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;

        if (currentProgress < lastProgress)
        {
            PlayWalkSound();
        }
        else if (lastProgress < 0.5f && currentProgress >= 0.5f)
        {
            PlayWalkSound();
        }

        lastProgress = currentProgress;
    }

    void PlayWalkSound()
    {
        if (audioSource == null || walkClips == null || walkClips.Length == 0) return;

        int index = UnityEngine.Random.Range(0, walkClips.Length);
        if (walkClips[index] != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.85f, 1.0f); 
            audioSource.PlayOneShot(walkClips[index]);
        }
    }

    void PlayAttackSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.05f); 
            audioSource.PlayOneShot(clip);
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
                    StartCoroutine(Attack1Routine());
                }
                else
                {
                    StartCoroutine(Attack2Routine());
                }
                lastAttackTime = Time.time;
            }
        }
    }

    //Attack 1 딜레이 관리
    IEnumerator Attack1Routine()
    {
        
        animator.SetTrigger("Attack1");

        // 딜레이
        yield return new WaitForSeconds(attack1Delay);

        // 딜레이 후 소리 재생
        PlayAttackSound(attack1Clip);
        
        if (playerScript != null)
        {
            playerScript.TakeDamage(attackDamage);
        }
    }

    IEnumerator Attack2Routine()
    {
        animator.SetTrigger("Attack2");
        
        yield return new WaitForSeconds(attack2Delay);
        
        PlayAttackSound(attack2Clip);
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

        // UI 연결
        OnHPChanged?.Invoke(currentHealth, maxHealth);
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

        // UI 연결
        HandlePlayerLost();
        
        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position, 1.0f);
        }
        
        animator.SetTrigger("Die"); 

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        // UI 관련 이벤트 함수 제거를 위한 보스 사라짐 알림
        BossManager.Instance.UnregisterBoss(this);

        // Kill Counter 반영
        KillCounter.Instance.AddBossKill();

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