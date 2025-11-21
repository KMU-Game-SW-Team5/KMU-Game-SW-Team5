using UnityEngine;

public class BossMonster : MonsterBase
{
    [Header("Boss Phase")]
    public bool hasEnteredPhase2 = false;
    public float phase2Threshold = 500f;   // 500 이하부터 2페이즈

    protected override void Awake()
    {
        base.Awake();

        // 보스는 높이 차도 그대로 바라보도록(Y 고정 X)
        lockYToSelf = false;
    }

    protected override void Update()
    {
        base.Update();
        // 상태이상 업데이트는 MonsterBase.Update()에서 처리
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!IsAlive || isDead)
        {
            SetMoveAnimation(false);
            return;
        }

        if (player == null)
        {
            SetMoveAnimation(false);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // 탐지 범위 안 & 공격 범위 밖 → 이동
        if (distance <= detectionRange && distance > attackRange)
        {
            MoveTowards(player.position);
            SetMoveAnimation(true);
        }
        // 공격 범위 안 → 공격 패턴 실행
        else if (distance <= attackRange)
        {
            TryAttack();
            SetMoveAnimation(false);
        }
        else
        {
            SetMoveAnimation(false);
        }
    }

    /// <summary>
    /// 체력 구간에 따라 공격 애니메이션/데미지 나누기
    /// (이전 BossController.AttackPlayer 로직 포팅)
    /// </summary>
    private void TryAttack()
    {
        if (!CanAttack() || playerScript == null)
            return;

        // 보스는 Y축 고정 없이 그대로 바라봄
        transform.LookAt(player.position);

        if (animator == null)
        {
            // 애니메이터가 없어도 데미지는 줄 수 있지만,
            // 보스면 사실상 애니메이션 없으면 이상하니 여기서 그냥 반환
            return;
        }

        if (currentHealth > 800f)
        {
            animator.SetTrigger("BasicAttack");
            playerScript.TakeDamage(attackDamage);
        }
        else if (currentHealth > 600f)
        {
            animator.SetTrigger("ClawAttack");
            playerScript.TakeDamage(attackDamage * 2);
        }
        else if (currentHealth > 500f)
        {
            animator.SetTrigger("FlameAttack");
            playerScript.TakeDamage(attackDamage * 3);
        }
        else
        {
            animator.SetTrigger("FlyAttack");
            playerScript.TakeDamage(attackDamage * 4);
        }

        lastAttackTime = Time.time;
    }

    /// <summary>
    /// 데미지를 받을 때 2페이즈 전환 체크까지 추가
    /// </summary>
    public override void TakeDamage(float dmg, GameObject attacker = null)
    {
        if (isDead) return;

        base.TakeDamage(dmg, attacker); // HP 감소 + 0 이하 시 Die 호출

        // 이미 죽었으면 추가 로직 안 타도록
        if (isDead) return;

        // 2페이즈 진입 체크
        if (!hasEnteredPhase2 && currentHealth <= phase2Threshold && currentHealth > 0)
        {
            hasEnteredPhase2 = true;

            // 2페이즈에서 공격 사거리 증가 (원래 50 → 100)
            attackRange = 100f;

            if (animator != null)
            {
                animator.SetBool("isPhase2", true);
                animator.SetTrigger("startPhase2");
            }

            Debug.Log($"{name}: Enter Phase 2");
        }
    }

    protected override void OnHit(GameObject attacker)
    {
        //if (animator != null)
        //    animator.SetTrigger("Hit");
    }

    protected override void OnDeath(GameObject killer)
    {
        // 보스 전용 드랍, 클리어 연출 등을 여기서 처리
        // Die 애니메이션 + Destroy는 MonsterBase.Die()에서 이미 처리됨.
        // 예: 보스 처치 UI, 포탈 생성 등
        // SpawnPortal();
        // GameManager.Instance.OnBossDefeated();
    }
}
