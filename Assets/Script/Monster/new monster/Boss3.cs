using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class Boss3 : BossMonsterBase
{
    [Header("Boss 3 Settings")]
    [SerializeField] private float detectionRangeOverride = 300f;
    [SerializeField] private float attackCooldownOverride = 2f;

    [Header("Boss 3 Attack Settings")]
    public GameObject bulletPrefab; 
    public Transform muzzleTransform; 
    public float spreadAngle = 3f;
    
    public float attack1Delay = 0.5f;
    public float attack2Delay = 0.5f; 

    [Header("Boss 3 Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips; 
    [SerializeField] private AudioClip deathClip;
    [SerializeField] private AudioClip attack1Clip; 
    [SerializeField] private AudioClip attack2Clip;

    // Boss3 고유 변수들
    private bool isPhase2 = false; 
    private float lastProgress = 0f; 

    protected void Start()
    {
        // 부모(MonsterBase)의 detectionRange에 override 값 반영
        if (detectionRangeOverride > 0f)
        {
            detectionRange = detectionRangeOverride;
        }

        if (attackCooldownOverride > 0f)
        {
            attackCooldown = attackCooldownOverride;
        }

        // 2페이즈 임계값을 maxHealth의 절반으로 설정
        phase2Threshold = maxHealth * 0.5f;

        // Rigidbody 초기화
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false; 
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        
        // AudioSource 초기화
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (animator == null) animator = GetComponentInChildren<Animator>();

        // 플레이어 찾기 시작
        InvokeRepeating("FindPlayer", 0f, 0.5f);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // 재활성화 대비: maxHealth가 변경되었을 수 있으므로 재계산
        phase2Threshold = maxHealth * 0.5f;
    }

    protected override void Update()
    {
        base.Update(); // 부모 Update 호출

        if (isDead) return;

        // [Boss3 고유 로직] 체력 50% 이하 시 2페이즈 전환 (phase2Threshold 사용)
        if (!isPhase2 && currentHealth <= phase2Threshold)
        {
            EnterPhase2();
        }
    }

    protected override void FixedUpdate()
    {
        if (isDead) return; 

        // 플레이어 유효성 검사
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
                MoveBossTowards(player.position);
                
                // UI 및 감지 처리
                InGameUIManager.Instance?.AppearBossUI(currentHealth, maxHealth);

                if (animator != null) animator.SetFloat("Speed", 1f); 

                // [Boss3 고유 로직] 발소리 체크
                CheckDualFootsteps();
            }
            else if (distance <= attackRange)
            {
                TryAttack();
                
                InGameUIManager.Instance?.AppearBossUI(currentHealth, maxHealth);
                if (animator != null) animator.SetFloat("Speed", 0f);
            }
            else
            {
                // 거리가 멀어졌을 때
                // InGameUIManager.Instance?.DisappearBossUI(); // 필요 시 주석 해제
                if (animator != null) animator.SetFloat("Speed", 0f);
            }
        }
        else
        {
            if (animator != null) animator.SetFloat("Speed", 0f);
        }
    }

    private void EnterPhase2()
    {
        isPhase2 = true;
        
        // 2페이즈 스탯 변경
        attackRange = 300f;   // 사거리 대폭 증가 (원거리 공격)
        detectionRange = 500f;
        attackDamage = 50; 
        attackCooldown = 2f; // 쿨타임 재설정
        
        Debug.Log("보스 3: 2페이즈 진입");
    }

    // Boss3Controller의 MoveTowardsPlayer -> MoveBossTowards
    protected override void MoveBossTowards(Vector3 target)
    {
        if (player == null) return; 

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

    // Boss3Controller의 AttackPlayer -> TryAttack
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
                // 페이즈 여부(혹은 체력)에 따라 공격 패턴 분기 (phase2Threshold 사용)
                if (currentHealth > phase2Threshold)
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

    // [Boss3 고유 로직] Attack 1 코루틴 (근접 공격 가정)
    IEnumerator Attack1Routine()
    {
        if (animator != null) animator.SetTrigger("Attack1");

        // 딜레이
        yield return new WaitForSeconds(attack1Delay);

        // 딜레이 후 소리 재생
        PlayAttackSound(attack1Clip);
        
        // 데미지 적용
        if (playerScript != null)
        {
            playerScript.TakeDamage(attackDamage);
        }
    }

    // [Boss3 고유 로직] Attack 2 코루틴 (원거리 투사체 공격)
    IEnumerator Attack2Routine()
    {
        if (animator != null) animator.SetTrigger("Attack2");
        
        yield return new WaitForSeconds(attack2Delay);
        
        PlayAttackSound(attack2Clip);
        ShootBullet();
    }

    // [Boss3 고유 로직] 투사체 발사
    private void ShootBullet()
    {
        if (bulletPrefab == null || muzzleTransform == null || player == null) return;

        // 플레이어 약간 위쪽을 조준
        Vector3 targetPosition = player.position + Vector3.up * 1.0f;
        Vector3 directionToPlayer = (targetPosition - muzzleTransform.position).normalized;
        Quaternion centerRotation = Quaternion.LookRotation(directionToPlayer);

        float halfSpread = spreadAngle / 2.0f;
        
        // 3갈래 발사
        Quaternion leftRotation = centerRotation * Quaternion.Euler(0, -halfSpread, 0);
        Quaternion rightRotation = centerRotation * Quaternion.Euler(0, halfSpread, 0);
        
        InstantiateBullet(centerRotation); 
        InstantiateBullet(leftRotation);   
        InstantiateBullet(rightRotation);  
    }

    private void InstantiateBullet(Quaternion rotation)
    {
        GameObject bulletGO = Instantiate(bulletPrefab, muzzleTransform.position, rotation);
        
        // Bullet 스크립트가 있다면 데미지 설정
        Bullet projectile = bulletGO.GetComponent<Bullet>(); 
        if (projectile != null)
        {
            projectile.Damage = attackDamage; 
        }
    }

    protected override void Die(GameObject killer = null)
    {
        if (isDead) return;
        isDead = true;
        PlayerLevelSystem.Instance?.AddExp(exp);


        Debug.Log("보스 3 사망");

        // UI 끄기
        InGameUIManager.Instance?.DisappearBossUI();
        
        if (deathClip != null)
        {
            AudioSource.PlayClipAtPoint(deathClip, transform.position, 1.0f);
        }
        
        if (animator != null) animator.SetTrigger("Die"); 

        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        KillCounter.Instance?.AddBossKill();


        spawnedRoom.NotifyMonsterDied(this.gameObject);

        Destroy(gameObject, 3f); // deathAnimationDuration
    }

    // -----------------------------------------------------------------------
    // Boss3 고유 기능 (발소리 체크, 오디오 재생)
    // -----------------------------------------------------------------------

    void CheckDualFootsteps()
    {
        if (animator == null) return;

        float currentProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;

        // 애니메이션 진행도가 이전보다 작아졌거나(루프), 0.5를 지나는 순간 발소리 재생
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

    // FindPlayer는 부모에 없다면 여기서 정의 (InvokeRepeating용)
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
}