using System;
using UnityEngine;

public abstract class BossMonsterBase : MonsterBase
{
    protected RoomManager spawnedRoom;
    public void SetRoom(RoomManager room)
    {
        spawnedRoom = room;
    }
    [Header("Boss Info")]
    [SerializeField] protected string bossName = "BossMonster";

    [Header("Boss Phase")]
    [SerializeField] protected bool hasEnteredPhase2 = false;
    [SerializeField] protected float phase2Threshold = 500f; // 이 값은 보스별로 인스펙터에서 조절
    [SerializeField] protected float phase2AttackRange = 100f; // 2페이즈 공격 사거리

    // [UI 연결] HP 변화 이벤트 
    private bool isPlayerDetected = false;

    protected override void Awake()
    {
        base.Awake();

        // 보스는 높이 차도 그대로 바라보도록 (Y 고정 X)
        lockYToSelf = false;
    }

    private void Start()
    {
        phase2Threshold = maxHealth * 0.5f; // 기본값: 최대체력의 50%
    }

    protected override void Update()
    {
        base.Update();
        // 상태이상 업데이트는 MonsterBase.Update()에서 처리한다고 가정
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
            MoveBossTowards(player.position);   // ★ 여기로 변경
            HandlePlayerDetected();
            SetMoveAnimation(true);
        }
        // 공격 범위 안 → 공격 패턴 실행
        else if (distance <= attackRange)
        {
            TryAttack();
            HandlePlayerDetected();
            SetMoveAnimation(false);
        }
        else
        {
            HandlePlayerLost();
            SetMoveAnimation(false);
        }
    }

    // 기본 구현: 그냥 MoveTowards로 위임
    protected virtual void MoveBossTowards(Vector3 target)
    {
        MoveTowards(target);
    }


    /// <summary>
    /// 보스별 공격 패턴은 여기서 구현하도록 강제
    /// </summary>
    protected abstract void TryAttack();

    /// <summary>
    /// 데미지를 받을 때 2페이즈 전환 체크까지 추가
    /// </summary>
    public override void TakeDamage(float dmg, GameObject attacker = null)
    {
        if (isDead) return;

        base.TakeDamage(dmg, attacker); // HP 감소 + 0 이하 시 Die 호출

        // UI 연결
        InGameUIManager.Instance.UpdateBossHPUI(currentHealth, maxHealth);

        // 이미 죽었으면 추가 로직 안 타도록
        if (isDead) return;

        // 2페이즈 진입 체크
        if (!hasEnteredPhase2 && currentHealth <= phase2Threshold && currentHealth > 0)
        {
            EnterPhase2();
        }
    }


    ///=================상태이상 제어 : 보스는 절반의 효과====================
    public override void TakeDOT(float dps, float duration)
    {
        base.TakeDOT(dps * 0.5f, duration);
    }

    public override void TakeSlow(float multiplier, float duration)
    {
        base.TakeSlow(multiplier * 0.5f, duration);
    }

    public override void TakeDebuff_AdditionalDamage(float rate, float duration)
    {
        base.TakeDebuff_AdditionalDamage(rate * 0.5f, duration);
    }
    public override void TakeStun(float duration)
    {
        base.TakeStun(duration * 0.5f);
    }

    /// <summary>
    /// 2페이즈에 들어갈 때 공통으로 해줄 행동
    /// </summary>
    protected virtual void EnterPhase2()
    {
        hasEnteredPhase2 = true;

    }

    protected override void Die(GameObject killer = null)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{name} died.");

        // 보스 전용 OnDeath 훅 (UI, 포탈 생성 등)
        OnDeath(killer);

        // 🔹 죽을 때는 비행 OFF → 중력 ON, 자연스럽게 추락
        SetFlyingMode(false);

        // 죽는 애니메이션
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 보스는 떨어져야 하므로 리지드바디를 동적으로 유지
        if (rb != null)
        {
            rb.isKinematic = false;   // 물리 시뮬 ON
                                      // rb.velocity는 상황에 따라 그대로 두는 게 더 자연스러움
        }

        // 콜라이더는 그대로 둬서 바닥에 부딪히게 할지,
        // 필요하면 isTrigger로 바꾸는 식으로 커스텀 가능

        Destroy(gameObject, deathAnimationDuration);
    }


    // 필요하다면 자식 보스에서 다시 override 가능
    protected override void OnHit(GameObject attacker)
    {
        // 기본 보스는 맞았을 때 특별히 아무것도 안 함
        // 자식 클래스에서 animator.SetTrigger("Hit") 같이 덮어써도 됨
        // UI 연결
        InGameUIManager.Instance.UpdateBossHPUI(currentHealth, maxHealth);
    }

    protected override void OnDeath(GameObject killer)
    {
        // 기본 보스는 여기서 특별한 연출 없음
        // 자식 클래스에서 보스 전용 드랍, 포탈 생성, 클리어 UI 등 구현
        // UI 연결
        HandlePlayerLost();
        // Kill Counter 반영
        KillCounter.Instance.AddBossKill();
    }

    // UI 연결
    void HandlePlayerDetected()
    {
        if (isPlayerDetected)
        {
            // 이미 감지 된 경우
            return;
        }
        else
        {
            // 새롭게 감지된 경우
            InGameUIManager.Instance.AppearBossUI(currentHealth, maxHealth, bossName);
            isPlayerDetected = true;
        }
    }

    // UI 연결
    void HandlePlayerLost()
    {
        isPlayerDetected = false;
        InGameUIManager.Instance.DisappearBossUI();    // UI
    }
}
