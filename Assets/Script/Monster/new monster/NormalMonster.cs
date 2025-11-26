using UnityEngine;
using UnityEngine.AI;

public class NormalMonster : MonsterBase
{
    private NavMeshAgent agent;

    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();

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
            if(agent.isOnNavMesh) agent.isStopped = true;
            SetMoveAnimation(false);
        }
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
        base.Die(killer);
        RoomManager room = GetComponentInParent<RoomManager>();
        room.NotifyMonsterDied(this.gameObject);
    }
}
