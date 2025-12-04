using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class NormalMonster : MonsterBase
{
    [Header("=== Patrol Settings ===")]
    [SerializeField] private float minPatrolMoveTime = 1f;
    [SerializeField] private float maxPatrolMoveTime = 3f;
    [SerializeField] private float patrolWaitTime = 2f;

    private float patrolTimer = 0f;
    private bool isPatrolMoving = false;
    private Vector3 patrolTargetPosition; 

    [Header("=== Audio Settings ===")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip deathClip;
    [Range(0f, 1f)] [SerializeField] private float walkSoundVolume = 0.5f;

    private float lastProgress = 0f;
    private float debugLogTimer = 0f; // 로그 도배 방지용

    protected override void Awake()
    {
        base.Awake(); 

        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
        }

        // Rigidbody 설정 강제 교정 (코드로 해결)
        if (rb != null)
        {
            rb.isKinematic = false; // 물리 켜기
            rb.useGravity = true;   // 중력 켜기
            // 회전만 잠그고 위치 이동은 허용
        }

        // 초기 배회 타이머
        patrolTimer = Random.Range(minPatrolMoveTime, maxPatrolMoveTime);
    }

    protected void Start()
    {
        if (player == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
            {
                player = pObj.transform;
                playerScript = pObj.GetComponent<Player>();
                Debug.Log($"<color=green>[{name}] 시작: Player 찾기 성공!</color>");
            }
        }
    }

    protected override void Update()
    {
        base.Update(); 

        // 발자국 소리
        if (IsAlive && !isStunned && rb != null)
        {
            // 실제 속도가 있을 때만 소리 재생
            if (rb.velocity.sqrMagnitude > 0.1f)
            {
                CheckDualFootsteps();
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate(); 

        // 1. 죽거나 스턴이면 정지
        if (!IsAlive || isStunned)
        {
            SetMoveAnimation(false);
            if (rb != null) rb.velocity = Vector3.zero;
            return;
        }

        // 2. 플레이어를 못 찾았으면 배회
        if (player == null)
        {
            DoPatrol();
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // ★ [디버깅] 1초마다 현재 상태 출력
        if (Time.time > debugLogTimer)
        {
            debugLogTimer = Time.time + 1f;
            string stateMsg = "배회 중";
            if (distance <= attackRange) stateMsg = "공격 범위";
            else if (distance <= detectionRange) stateMsg = "추격 중 (이동해야 함)";
            
            Debug.Log($"[{name}] 거리: {distance:F1} (감지: {detectionRange}) -> 상태: {stateMsg}");
        }

        // 3. [추격] 감지 범위 안 + 공격 범위 밖
        if (distance <= detectionRange && distance > attackRange)
        {
            // ★ 이동 명령
            MoveTowards(player.position);
            SetMoveAnimation(true);
        }
        // 4. [공격] 공격 범위 안
        else if (distance <= attackRange)
        {
            if (rb != null) rb.velocity = Vector3.zero; // 공격할 땐 정지
            SetMoveAnimation(false);
            TryAttack();
        }
        // 5. [배회]
        else
        {
            DoPatrol();
        }
    }

    // ★ 이동 로직 (부모 클래스 대신 직접 작성하여 문제 원인 차단)
    protected new void MoveTowards(Vector3 target)
    {
        if (rb == null) return;

        // 1. 바라보기
        Vector3 lookPos = target;
        lookPos.y = transform.position.y; // 높이는 무시하고 수평 회전
        transform.LookAt(lookPos);

        // 2. 이동 (MovePosition 사용)
        Vector3 dir = (target - transform.position).normalized;
        
        // 현재 위치에서 목표 방향으로 조금 이동한 위치 계산
        Vector3 nextPos = transform.position + (dir * moveSpeed * Time.fixedDeltaTime);
        
        // Y축(높이)은 현재 높이 유지 (바닥 꺼짐 방지)
        nextPos.y = transform.position.y; 

        rb.MovePosition(nextPos);
    }

    private void DoPatrol()
    {
        patrolTimer -= Time.fixedDeltaTime;

        if (patrolTimer <= 0f)
        {
            if (isPatrolMoving)
            {
                isPatrolMoving = false;
                patrolTimer = patrolWaitTime;
                SetMoveAnimation(false);
            }
            else
            {
                isPatrolMoving = true;
                patrolTimer = Random.Range(minPatrolMoveTime, maxPatrolMoveTime);

                Vector2 randomCircle = Random.insideUnitCircle.normalized;
                Vector3 randomDir = new Vector3(randomCircle.x, 0, randomCircle.y);
                patrolTargetPosition = transform.position + randomDir * 5f;
            }
        }

        if (isPatrolMoving)
        {
            MoveTowards(patrolTargetPosition);
            SetMoveAnimation(true);
        }
    }

    private void TryAttack()
    {
        if (!CanAttack() || playerScript == null) return;

        Vector3 lookPos = player.position;
        if (lockYToSelf) lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (animator != null) animator.SetTrigger("BasicAttack"); 
        PlayOneShotSound(attackClip);
        ApplyBasicAttackToPlayer();
    }

    protected override void Die(GameObject killer = null)
    {
        if (isDead) return;
        if (deathClip != null) AudioSource.PlayClipAtPoint(deathClip, transform.position, 1.0f);
        base.Die(killer);
        RoomManager room = GetComponentInParent<RoomManager>();
        if (room != null) room.NotifyMonsterDied(this.gameObject);
    }

    private void PlayOneShotSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }

    private void CheckDualFootsteps()
    {
        if (animator == null) return;
        float currentProgress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1.0f;
        if (currentProgress < lastProgress) PlayWalkSound();
        else if (lastProgress < 0.5f && currentProgress >= 0.5f) PlayWalkSound();
        lastProgress = currentProgress;
    }

    private void PlayWalkSound()
    {
        if (audioSource == null || walkClips == null || walkClips.Length == 0) return;
        int index = Random.Range(0, walkClips.Length);
        if (walkClips[index] != null)
        {
            audioSource.pitch = Random.Range(0.8f, 1.1f);
            audioSource.PlayOneShot(walkClips[index], walkSoundVolume);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}