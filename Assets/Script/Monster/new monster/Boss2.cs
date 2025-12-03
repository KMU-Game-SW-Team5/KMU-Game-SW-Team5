using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Boss2 : BossMonsterBase
{
    [Header("Boss 2 Settings")]
    [SerializeField] private float detectionRangeOverride = 300f;
    [SerializeField] private float attackCooldownOverride = 2f;
    
    // 이동 히스테리시스용 변수
    private bool isMovingState = false; 

    [Header("Boss 2 Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip moveLoopClip;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private AudioClip attack1Clip;
    [SerializeField] private AudioClip attack2Clip;
    [SerializeField] private AudioClip deathClip;

    private float originalVolume = 0.3f;
    private Coroutine currentFadeCoroutine;

    protected void Start()
    {
        // 부모(MonsterBase)의 필드에 override 값 반영
        if (detectionRangeOverride > 0f)
        {
            detectionRange = detectionRangeOverride;
        }

        if (attackCooldownOverride > 0f)
        {
            attackCooldown = attackCooldownOverride;
        }

        // 초기화 안전 장치
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

        // [Boss2 설정] Rigidbody 제약
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        // [Boss2 설정] 오디오 초기화
        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.clip = moveLoopClip;
            audioSource.playOnAwake = false;
            audioSource.volume = 0f;
        }

        // 플레이어 찾기 시작
        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    protected override void FixedUpdate()
    {
        if (isDead)
        {
            if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
            return;
        }

        // 플레이어 유효성 검사
        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
            playerScript = null;
        }
        
        bool currentIsMoving = false;

        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            // [이동 로직] 히스테리시스 (Start/Stop Threshold)
            float stopThreshold = attackRange;
            float startThreshold = attackRange + 1.0f;

            bool shouldMove = isMovingState
                ? (distance > stopThreshold)
                : (distance > startThreshold);

            // 1. 이동 판정
            if (distance <= detectionRange && shouldMove)
            {
                MoveBossTowards(player.position);
                
                // UI 연결
                InGameUIManager.Instance?.AppearBossUI(currentHealth, maxHealth);

                if (animator != null) animator.SetFloat("Speed", 1f);

                currentIsMoving = true;
                isMovingState = true;
            }
            else
            {
                // 2. 공격 판정
                if (distance <= attackRange)
                {
                    TryAttack();
                    InGameUIManager.Instance?.AppearBossUI(currentHealth, maxHealth);
                }
                
                if (animator != null) animator.SetFloat("Speed", 0f);
                
                currentIsMoving = false;
                isMovingState = false;
            }
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
            currentIsMoving = false;
        }

        // 이동 사운드 페이드 처리
        HandleFadeSound(currentIsMoving);
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

    protected override void MoveBossTowards(Vector3 target)
    {
        // Y축 고정 시선 처리
        Vector3 lookPosition = target;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);

        // Y축 고정 이동 처리
        Vector3 targetPosition = target;
        targetPosition.y = transform.position.y;

        Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);
    }

    protected override void TryAttack()
    {
        // 시선 고정
        Vector3 lookPosition = player.position;
        lookPosition.y = transform.position.y;
        transform.LookAt(lookPosition);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            if (playerScript != null)
            {
                if (currentHealth > maxHealth / 2)
                {
                    if (animator != null) animator.SetTrigger("Attack1");
                    PlayGlobalSound(attack1Clip);
                    playerScript.TakeDamage(attackDamage);
                }
                else
                {
                    if (animator != null) animator.SetTrigger("Attack2");
                    PlayGlobalSound(attack2Clip);
                    playerScript.TakeDamage(attackDamage * 2);
                }
                lastAttackTime = Time.time;
            }
        }
    }

    protected override void Die(GameObject killer = null)
    {
        if (isDead) return;
        isDead = true;

        Debug.Log("보스 2 사망");

        InGameUIManager.Instance?.DisappearBossUI();

        StopAllCoroutines();
        if (audioSource != null) audioSource.Stop();

        PlayGlobalSound(deathClip);

        if (animator != null) animator.SetTrigger("Die");
        
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        var col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        KillCounter.Instance?.AddBossKill();

        spawnedRoom.NotifyMonsterDied(this.gameObject);

        Destroy(gameObject, 3f); 
    }

    void HandleFadeSound(bool isMoving)
    {
        if (audioSource == null || moveLoopClip == null) return;

        if (isMoving)
        {
            if (!audioSource.isPlaying || audioSource.volume < originalVolume)
            {
                if (!audioSource.isPlaying) audioSource.Play();
                StartFade(originalVolume);
            }
        }
        else
        {
            if (audioSource.isPlaying && audioSource.volume > 0f)
            {
                StartFade(0f);
            }
        }
    }

    void StartFade(float targetVolume)
    {
        if (currentFadeCoroutine != null) StopCoroutine(currentFadeCoroutine);
        currentFadeCoroutine = StartCoroutine(FadeAudio(targetVolume));
    }

    IEnumerator FadeAudio(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }

        audioSource.volume = targetVolume;

        if (targetVolume <= 0.01f)
        {
            audioSource.Stop();
        }
    }

    void PlayGlobalSound(AudioClip clip)
    {
        if (clip == null) return;

        GameObject tempAudio = new GameObject("TempAudio_Global");
        tempAudio.transform.position = transform.position;

        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.spatialBlend = 0f; 
        tempSource.volume = 0.5f;

        tempSource.Play();
        Destroy(tempAudio, clip.length);
    }
}