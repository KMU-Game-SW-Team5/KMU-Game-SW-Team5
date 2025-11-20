using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Boss2Controller : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    public float detectionRange = 300f;
    [SerializeField] private float attackRange = 30f;
    public float moveSpeed = 30f;
    public int maxHealth = 1000;
    private int currentHealth;

    [Header("Combat")]
    public int attackDamage = 10;
    public float attackCooldown = 2f;
    private float lastAttackTime;

    [Header("State")]
    private float deathAnimationDuration = 3f;
    private bool isDead = false;
    
    private Transform player;
    private Player playerScript;
    private Rigidbody rb;
    private Animator animator;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveLoopClip;
    
    // [수정] 페이드 효과 지속 시간 (0.5초 동안 서서히 켜지고 꺼짐)
    [SerializeField] private float fadeDuration = 0.5f; 
    private float originalVolume; // 원래 설정해둔 볼륨 크기 저장용
    private Coroutine currentFadeCoroutine; // 현재 실행 중인 페이드 코루틴 저장

    private bool isMovingState = false; 

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false; 
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        animator = GetComponentInChildren<Animator>();

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        
        // 오디오 초기 설정
        audioSource.loop = true;
        audioSource.clip = moveLoopClip;
        audioSource.playOnAwake = false;
        
        // 원래 볼륨 저장해두고, 시작할 땐 0으로 세팅 (페이드 인을 위해)
        originalVolume = audioSource.volume; 
        audioSource.volume = 0f;

        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (isDead) 
        {
            if (audioSource.isPlaying) audioSource.Stop();
            return; 
        }

        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
            playerScript = null;
        }
        
        bool currentIsMoving = false;

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            // 떨림 방지 로직 (Hysteresis)
            float stopThreshold = attackRange;
            float startThreshold = attackRange + 1.0f; 

            bool shouldMove = isMovingState 
                ? (distance > stopThreshold) 
                : (distance > startThreshold);

            if (distance <= detectionRange && shouldMove)
            {
                MoveTowardsPlayer();
                if(animator != null) animator.SetFloat("Speed", 1f); 
                
                currentIsMoving = true;
                isMovingState = true;
            }
            else
            {
                if (distance <= attackRange) AttackPlayer();
                if(animator != null) animator.SetFloat("Speed", 0f);
                
                currentIsMoving = false;
                isMovingState = false; 
            }
        }
        else
        {
            if(animator != null) animator.SetFloat("Speed", 0f);
            currentIsMoving = false;
        }

        if(currentHealth <= 500)
        {
            attackDamage = 20;
            attackCooldown = 2f;
            attackRange = 130f;
        }

        // [수정] 페이드 효과가 적용된 사운드 처리 함수 호출
        HandleFadeSound(currentIsMoving);
    }

    // [핵심] 페이드 인/아웃 관리 함수
    void HandleFadeSound(bool isMoving)
    {
        if (audioSource == null || moveLoopClip == null) return;

        if (isMoving)
        {
            // 움직이는데 소리가 안 나거나, 볼륨이 작다면 -> 페이드 인 (소리 키우기)
            if (!audioSource.isPlaying || audioSource.volume < originalVolume)
            {
                if (!audioSource.isPlaying) audioSource.Play(); // 일단 재생 시작
                
                // 이미 페이드 아웃 중이었다면 멈추고 페이드 인으로 전환
                StartFade(originalVolume); 
            }
        }
        else
        {
            // 멈췄는데 소리가 나고 있다면 -> 페이드 아웃 (소리 줄이기)
            if (audioSource.isPlaying && audioSource.volume > 0f)
            {
                StartFade(0f);
            }
        }
    }

    // 코루틴 시작 도우미 함수
    void StartFade(float targetVolume)
    {
        // 이미 진행 중인 페이드가 있다면 취소 (중복 실행 방지)
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        
        // 새 페이드 시작
        currentFadeCoroutine = StartCoroutine(FadeAudio(targetVolume));
    }

    // 실제 서서히 볼륨을 조절하는 코루틴
    IEnumerator FadeAudio(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            // lerp를 이용해 부드럽게 볼륨 조절
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        // 목표 볼륨이 0이면 (완전히 꺼지면) Stop() 호출해서 리소스 아끼기
        if (targetVolume <= 0.01f)
        {
            audioSource.Stop();
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
                    animator.SetTrigger("Attack2");
                    playerScript.TakeDamage(attackDamage * 2);
                }
                lastAttackTime = Time.time;
            }
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
        
        // 죽을 때도 페이드 아웃으로 부드럽게 끄기
        StartFade(0f);

        animator.SetTrigger("Die"); 
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;
        Destroy(gameObject, deathAnimationDuration);
    }
}