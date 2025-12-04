using UnityEngine;
using System.Collections;

// NavMeshAgent를 사용하지 않고 Rigidbody로만 이동할 경우,
// 장애물 충돌은 Rigidbody와 Collider에 의존합니다.
// 몬스터가 겹치지 않게 하려면 Rigidbody의 Y축 움직임을 제한합니다.
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

        // Rigidbody 설정 강제 교정
        if (rb != null)
        {
            rb.isKinematic = false; // 물리 켜기
            rb.useGravity = true;   // 중력 켜기
            
            // ★ 수정: 몬스터가 다른 몬스터 위로 올라타거나 공중에 뜨는 것을 방지
            // Y축 위치 잠금 (FreezePositionY) 추가
            rb.constraints = RigidbodyConstraints.FreezeRotationX | 
                             RigidbodyConstraints.FreezeRotationZ |
                             RigidbodyConstraints.FreezePositionY; // Y축 위치 이동 잠금
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

    // ★ 수정된 이동 로직 (맵 통과 버그 완화를 위해 velocity 사용)
    protected new void MoveTowards(Vector3 target)
    {
        if (rb == null) return;

        // 1. 바라보기
        Vector3 lookPos = target;
        lookPos.y = transform.position.y; // 높이는 무시하고 수평 회전
        transform.LookAt(lookPos);

        // 2. 이동 (velocity 사용: 물리 엔진에 이동을 맡겨 충돌 감지 활용)
        Vector3 dir = (target - transform.position).normalized;
        // Y축 속도는 0으로 설정하여 현재 높이 유지 (FreezePositionY와 시너지)
        rb.velocity = new Vector3(dir.x * moveSpeed, 0f, dir.z * moveSpeed);
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
                // 이동이 끝났으면 멈추기
                if (rb != null) rb.velocity = Vector3.zero; 
            }
            else
            {
                isPatrolMoving = true;
                patrolTimer = Random.Range(minPatrolMoveTime, maxPatrolMoveTime);

                Vector2 randomCircle = Random.insideUnitCircle.normalized;
                Vector3 randomDir = new Vector3(randomCircle.x, 0, randomCircle.y);
                patrolTargetPosition = transform.position + randomDir * 5f;
                // ★ 수정: 목표 위치의 Y축을 현재 몬스터의 Y축으로 고정
                patrolTargetPosition.y = transform.position.y;
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