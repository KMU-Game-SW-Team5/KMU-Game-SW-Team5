using UnityEngine;

public class NormalMonster : MonsterBase
{
    private float checkInterval = 0.5f;    // 이동 확인 주기 (1초)
    private float minMoveDistance = 0.5f;  // 이 거리 이하면 막힌 걸로 간주
    private float avoidDuration = 2f;    // 회피 기동 지속 시간


    private float stuckCheckTimer = 0f;
    private Vector3 lastPosition;
    
    private bool isAvoiding = false;    // 현재 회피 중인가?
    private float avoidTimer = 0f;      // 회피 남은 시간
    private Vector3 avoidDirection;     // 회피할 방향

    protected override void Awake()
    {
        base.Awake();
        lastPosition = transform.position;
    }

    protected override void Update()
    {
        base.Update();
        // 상태이상(슬로우/스턴/DOT)은 MonsterBase.Update()에서 처리
        // 일반 몬스터의 AI 로직은 FixedUpdate에서 처리
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // 죽었으면 아무 것도 안 함
        if (!IsAlive || isDead)
        {
            SetMoveAnimation(false);
            return;
        }

        // 타겟이 없으면 이동/공격 없음
        if (player == null)
        {
            SetMoveAnimation(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // 탐지 범위 안이면서, 공격 범위 밖 → 이동
        if (distance <= detectionRange && distance > attackRange)
        {
            // 끼임 감지 체크
            CheckIfStuck();
            if (isAvoiding)
            {
                // [회피 모드] 계산된 회피 방향으로 잠시 이동
                // MoveTowards는 목적지 좌표를 받으므로, 현재 위치에서 멀리 떨어진 지점을 타겟으로 설정
                Vector3 avoidTarget = transform.position + avoidDirection * 5f;
                MoveTowards(avoidTarget);

                // 회피 타이머 감소
                avoidTimer -= Time.fixedDeltaTime;
                if (avoidTimer <= 0)
                {
                    isAvoiding = false; // 시간 종료 시 다시 추격 모드로 복귀
                }
            }
            else
            {
                // [일반 추격 모드] 플레이어를 향해 이동
                MoveTowards(player.position);
            }

            SetMoveAnimation(true);   // Animator "Speed" = 1
        }
        // 공격 범위 안 → 공격 시도
        else if (distance <= attackRange)
        {
            ResetStuckCheck();
            TryAttack();
        }
        // 그 외(너무 멀거나 아직 못 찾았거나) → 대기
        else
        {
            ResetStuckCheck();
            SetMoveAnimation(false);
        }
    }
    // 끼임 감지 로직
    private void CheckIfStuck()
    {
        if (isAvoiding) return;

        stuckCheckTimer += Time.fixedDeltaTime;

        // 1초 마다 검사
        if (stuckCheckTimer >= checkInterval)
        {
            float movedDist = Vector3.Distance(transform.position, lastPosition);

            
            if (movedDist < minMoveDistance)
            {
                StartAvoidance();
            }

            
            lastPosition = transform.position;
            stuckCheckTimer = 0f;
        }
    }
    private void StartAvoidance()
    {
        isAvoiding = true;
        avoidTimer = avoidDuration;

        // 플레이어 방향을 기준으로 좌/우 랜덤으로 꺾어서 탈출 시도
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        
        
        float randomAngle = Random.Range(45f, 135f);
        if (Random.value > 0.5f) randomAngle *= -1; // 왼쪽 or 오른쪽 랜덤

        
        avoidDirection = Quaternion.Euler(0, randomAngle, 0) * dirToPlayer;
    }

    private void ResetStuckCheck()
    {
        stuckCheckTimer = 0f;
        lastPosition = transform.position;
        isAvoiding = false;
    }

    private void TryAttack()
    {
        // 제자리에서 공격 애니메이션 재생
        SetMoveAnimation(false);

        // 쿨타임 미충족
        if (!CanAttack())
            return;

        // 플레이어 스크립트 없으면 공격 불가
        if (playerScript == null)
            return;

        // 공격 시에도 Y축은 자기 높이에 고정해서 바라보게 (MonsterController와 동일 컨셉)
        Vector3 lookPos = player.position;
        if (lockYToSelf)
            lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        // 공격 애니메이션
        if (animator != null)
            animator.SetTrigger("Attack");

        // 실제 데미지 적용은 MonsterBase의 공통 함수 사용
        ApplyBasicAttackToPlayer();
    }

    protected override void OnHit(GameObject attacker)
    {
        // 맞았을 때 반응(넉백, 피격 애니메이션 등)을 원하면 여기서 구현
        // 예시:
        //if (animator != null)
        //    animator.SetTrigger("Hit");
    }

    protected override void OnDeath(GameObject killer)
    {
        // 사망 시 드랍 아이템, 경험치, 점수 처리 등을 여기서 구현
        // MonsterBase.Die()가 이미:
        //  - isDead = true
        //  - "Die" 트리거
        //  - Rigidbody 정지 + Collider 비활성화
        //  - deathAnimationDuration 후 Destroy
        // 까지 처리해 줌
    }
    protected override void Die(GameObject killer = null)
    {
        base.Die(killer);
        RoomManager room = GetComponentInParent<RoomManager>();
        room.NotifyMonsterDied(this.gameObject);
    }
}
