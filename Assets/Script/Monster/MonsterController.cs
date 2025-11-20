using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class MonsterController : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float detectionRange = 100f;
    public float attackRange = 10f;    
    public float moveSpeed = 10f;    
    public int maxHealth = 1000;
    public int attackDamage = 5;      
    public float attackCooldown = 2.0f; 
    public float attackDelay = 0.5f; 

    private float deathAnimationDuration = 3f;
    private int currentHealth;
    private float lastAttackTime;
    private bool isDead = false;

    private Transform player;
    private Player playerScript;
    private Rigidbody rb;
    private Animator animator;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips; 
    [SerializeField] private AudioClip attackClip;  
    [SerializeField] private AudioClip deathClip;   

    private float lastProgress = 0f; 

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponentInChildren<Animator>();
        if (animator == null) Debug.LogError("몬스터: Animator 없음");

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

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
            if (animator != null) animator.SetFloat("Speed", 0f);
        }
    }

    void CheckDualFootsteps()
    {
        if (animator == null) return;
        float currentProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;

        if (currentProgress < lastProgress) PlayWalkSound();
        else if (lastProgress < 0.5f && currentProgress >= 0.5f) PlayWalkSound();

        lastProgress = currentProgress;
    }

    void PlayWalkSound()
    {
        if (audioSource == null || walkClips == null || walkClips.Length == 0) return;

        int index = Random.Range(0, walkClips.Length);
        if (walkClips[index] != null)
        {
            // 걷기는 여전히 3D (거리감 유지) - 볼륨만 좀 크게
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(walkClips[index], 4.0f);
        }
    }

    // ▼▼▼ [추가] 2D 사운드 재생 전용 함수 (공격용) ▼▼▼
    void Play2DSound(AudioClip clip)
    {
        if (clip == null) return;

        // 임시 오브젝트 생성
        GameObject tempGO = new GameObject("Temp2DSound");
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        
        tempSource.clip = clip;
        tempSource.spatialBlend = 0f; // ★ 0으로 하면 완전 2D (거리 상관없이 똑같이 들림) ★
        tempSource.volume = 1.0f;     // 최대 볼륨
        
        tempSource.Play();
        Destroy(tempGO, clip.length); // 재생 후 자동 삭제
    }
    // ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲

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
                StartCoroutine(AttackRoutine());
                lastAttackTime = Time.time;
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        if(animator != null) animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackDelay);

        // ▼▼▼ [수정] 공격 소리는 이제 2D로 재생! ▼▼▼
        if (attackClip != null)
        {
            Play2DSound(attackClip);
        }

        if (playerScript != null)
        {
            Debug.Log("몬스터 공격 적중!");
            playerScript.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("몬스터가 쓰러졌습니다.");
        
        // 사망 소리는 거리가 느껴지게 3D로 유지 (5배 크기)
        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position, 5.0f);
        }

        if(animator != null) animator.SetTrigger("Die");

        rb.isKinematic = true; 
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false; 

        Destroy(gameObject, deathAnimationDuration);
    }
}