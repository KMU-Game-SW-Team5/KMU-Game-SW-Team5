using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
public class NormalMonster : MonsterBase
{
    private NavMeshAgent agent;

    private Vector3 patrolTarget;         
    private float patrolRange = 15f;      // 몬스터의 배회 범위
    private float patrolWaitTime = 0.3f;  // 대기 시간
    private float lastPatrolTime;         
    private float minPatrolMoveDistance = 0.5f;

    private Queue<Vector3> visitedPatrolPoints = new Queue<Vector3>(); // 방문 지점을 저장할 큐
    private const int MAX_VISITED_POINTS = 7;                          // 기억할 최대 지점 수
    private const float PATROL_AVOIDANCE_RADIUS = 5f;
    

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();

        detectionRange /= 2f;

        // 몬스터 설정 (회전은 직접 제어, 2D/3D에 따라 UpAxis 설정)
        agent.updateRotation = false; 
        agent.updateUpAxis = false; // 3D 게임이면 false
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

        if (!IsAlive || isDead || player == null || isStunned)
        {
            if(agent.isOnNavMesh) agent.isStopped = true;
            SetMoveAnimation(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // [이동] 탐지 범위 안, 공격 범위 밖
        if (distance <= detectionRange && distance > attackRange)
        {
            // NavMesh 위에서만 이동 명령
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position); // 알아서 장애물 피해서 감
            }

            // [회전] 가야 할 방향 바라보기
            if (agent.velocity.sqrMagnitude > 0.1f)
            {
                Vector3 dir = agent.steeringTarget - transform.position;
                dir.y = 0;
                if (dir != Vector3.zero)
                {
                    Quaternion rot = Quaternion.LookRotation(dir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.fixedDeltaTime * 10f);
                }
            }
            SetMoveAnimation(true);
        }
        // [공격] 공격 범위 안
        else if (distance <= attackRange)
        {
            if(agent.isOnNavMesh) agent.isStopped = true; // 멈춤
            TryAttack();
        }
        else
        {
            DoPatrol();
        }
    }
    // --- DoPatrol() 함수 ---
    private void DoPatrol()
    {
        if (!agent.isOnNavMesh) 
        {
            SetMoveAnimation(false);
            return;
        }

        // 목표 도달 거리가 가까워졌거나, 대기 시간이 지났다면 새로운 목표 설정
        if (agent.remainingDistance <= minPatrolMoveDistance || Time.time > lastPatrolTime + patrolWaitTime)
        {
            // 목표에 도달했을 때만 해당 지점을 큐에 기록 (새로운 목표 설정 직전에 실행)
            if (agent.remainingDistance <= minPatrolMoveDistance && patrolTarget != Vector3.zero)
            {
                visitedPatrolPoints.Enqueue(patrolTarget);
                // 4개를 초과하면 가장 오래된 지점 제거
                if (visitedPatrolPoints.Count > MAX_VISITED_POINTS)
                {
                    visitedPatrolPoints.Dequeue();
                }
            }

            // 목표에 도착했으므로 잠시 멈춤
            agent.isStopped = true;
            SetMoveAnimation(false);

            lastPatrolTime = Time.time;

            // 대기 시간이 짧거나 0에 가깝다면 즉시 다음 목표를 설정
            if (patrolWaitTime <= 0.1f)
            {
                SetNewPatrolTarget();
            }
            else
            {
                // 설정된 대기 시간만큼 멈춘 후 목표 설정
                Invoke("SetNewPatrolTarget", patrolWaitTime); 
            }
        }
        else if(agent.isStopped)
        {
            // 목표를 찾았으나 대기 중인 상태일 때
            SetMoveAnimation(false);
        }
        else
        {
            // 이동 중일 때 회전 (추격과 동일한 회전 로직 사용)
            Vector3 dir = agent.steeringTarget - transform.position;
            dir.y = 0;
            if (dir != Vector3.zero)
            {
                Quaternion rot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.fixedDeltaTime * 10f);
            }
            SetMoveAnimation(true);
        }
    }

    private void SetNewPatrolTarget()
    {
        const int MAX_ATTEMPTS = 10; // 무한 루프 방지를 위한 최대 시도 횟수

        for (int i = 0; i < MAX_ATTEMPTS; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * patrolRange;
            randomDirection += transform.position;
            
            NavMeshHit hit;
            // NavMesh 위에서 유효한 위치를 찾습니다.
            if (NavMesh.SamplePosition(randomDirection, out hit, patrolRange, NavMesh.AllAreas))
            {
                // [핵심] 최근 방문했던 지점과 너무 가까운지 확인합니다.
                if (!IsTooCloseToVisited(hit.position, PATROL_AVOIDANCE_RADIUS))
                {
                    // 유효한 위치를 찾았습니다.
                    patrolTarget = hit.position;
                    agent.SetDestination(patrolTarget);
                    agent.isStopped = false;
                    SetMoveAnimation(true);
                    return; // 함수 종료
                }
            }
        }
        
        // 10번 시도했으나 유효한 위치를 찾지 못했다면, 그냥 멈춰있습니다.
        // 다음 FixedUpdate 루프에서 다시 시도됩니다.
    }
    /// <summary>
    /// 새로운 위치가 최근 방문했던 지점과 너무 가까운지 확인합니다.
    /// </summary>
    private bool IsTooCloseToVisited(Vector3 position, float safeDistance)
    {
        foreach (Vector3 visited in visitedPatrolPoints)
        {
            // Y축을 무시하고 수평 거리만 비교합니다 (바닥 높이가 달라도 같은 위치로 간주)
            Vector3 p1 = new Vector3(position.x, 0, position.z);
            Vector3 p2 = new Vector3(visited.x, 0, visited.z);
            
            if (Vector3.Distance(p1, p2) < safeDistance)
            {
                return true; // 너무 가까움
            }
        }
        return false;
    }
    
    private void TryAttack()
    {
        SetMoveAnimation(false);
        if (!CanAttack() || playerScript == null) return;

        // 공격 시 플레이어 보기
        Vector3 lookPos = player.position;
        if (lockYToSelf) lookPos.y = transform.position.y;
        transform.LookAt(lookPos);

        if (animator != null) animator.SetTrigger("Attack");
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
        if (isDead) return;

        base.Die(killer);

        // 길막 방지
        if (agent != null) 
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
            agent.enabled = false; 
        }

        
        UnityEngine.AI.NavMeshObstacle obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (obstacle != null) obstacle.enabled = false;

        
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // 물리 연산 제거 충돌 감지 끄기
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;       // 물리 힘 무시
            rb.detectCollisions = false; // 충돌 계산 자체를 끔
        }

        ChangeLayerRecursively(this.transform, "Ignore Raycast");

        // ---------------------------------------------------------

        RoomManager room = GetComponentInParent<RoomManager>();
        if (room != null) room.NotifyMonsterDied(this.gameObject);
    }

    
    private void ChangeLayerRecursively(Transform trans, string layerName)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        if (layerIndex == -1) return; 

        trans.gameObject.layer = layerIndex;
        foreach (Transform child in trans)
        {
            ChangeLayerRecursively(child, layerName);
        }
    }
}
