using UnityEngine;

public class NormalMonster : MonsterBase
{
    protected override void Awake()
    {
        base.Awake();
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
            // MonsterBase의 MoveTowards는 슬로우/스턴/리지드바디/Y축 고정까지 처리
            MoveTowards(player.position);
            SetMoveAnimation(true);   // Animator "Speed" = 1
        }
        // 공격 범위 안 → 공격 시도
        else if (distance <= attackRange)
        {
            TryAttack();
        }
        // 그 외(너무 멀거나 아직 못 찾았거나) → 대기
        else
        {
            SetMoveAnimation(false);
        }
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
