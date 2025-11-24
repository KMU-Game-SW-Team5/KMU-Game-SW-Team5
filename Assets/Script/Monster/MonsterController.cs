using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    // [추가] 2연타 공격 설정
    [Header("Double Attack Settings")]
    public bool enableDoubleAttack = false; 
    public float secondAttackDelay = 0.5f;  

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
    [SerializeField] private AudioClip secondAttackClip; 
    [SerializeField] private AudioClip deathClip;   

    private float lastProgress = 0f; 
    private GameObject currentAttackSoundObject; 

    
    private Coroutine runningAttackCoroutine; 
    

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
                CancelAttack();
                

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

    void CancelAttack()
    {
        if (runningAttackCoroutine != null)
        {
            StopCoroutine(runningAttackCoroutine);
            runningAttackCoroutine = null;
        }

        
        if (currentAttackSoundObject != null)
        {
            Destroy(currentAttackSoundObject);
            currentAttackSoundObject = null;
        }

        
        if (animator != null)
        {
            animator.ResetTrigger("Attack");
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
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(walkClips[index], 4.0f);
        }
    }

    void Play2DSound(AudioClip clip)
    {
        if (clip == null) return;
        if (currentAttackSoundObject != null) Destroy(currentAttackSoundObject);

        currentAttackSoundObject = new GameObject("Temp2DSound");
        AudioSource tempSource = currentAttackSoundObject.AddComponent<AudioSource>();
        
        tempSource.clip = clip;
        tempSource.spatialBlend = 0f; 
        tempSource.volume = 1.0f;     
        
        tempSource.Play();
        Destroy(currentAttackSoundObject, clip.length); 
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
                // [수정] 코루틴 시작 시 변수에 저장 (취소할 수 있게)
                runningAttackCoroutine = StartCoroutine(AttackRoutine());
                lastAttackTime = Time.time;
            }
        }
    }

    IEnumerator AttackRoutine()
    {
        if(animator != null) 
        {
            animator.ResetTrigger("Attack");
            animator.SetTrigger("Attack");
        }

        // 1타 대기
        yield return new WaitForSeconds(attackDelay);

        if (attackClip != null) Play2DSound(attackClip);

        if (playerScript != null)
        {
            Debug.Log("몬스터 1타 적중!");
            playerScript.TakeDamage(attackDamage);
        }

        // 2타 (옵션)
        if (enableDoubleAttack)
        {
            yield return new WaitForSeconds(secondAttackDelay);

            AudioClip clip2 = (secondAttackClip != null) ? secondAttackClip : attackClip;
            Play2DSound(clip2);

            if (playerScript != null)
            {
                Debug.Log("몬스터 2타 적중!");
                playerScript.TakeDamage(attackDamage);
            }
        }

        // 공격이 정상적으로 끝나면 변수 비우기
        runningAttackCoroutine = null;
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
        
        CancelAttack();
        
        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position, 5.0f);
        }

        if(animator != null) animator.SetTrigger("Die");

        rb.isKinematic = true; 
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        // Kill Counter 반영
        KillCounter.Instance.AddMonsterKill();

        Destroy(gameObject, deathAnimationDuration);
    }
}