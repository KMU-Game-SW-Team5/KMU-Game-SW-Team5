using UnityEngine;

public abstract class MonsterBase : MonoBehaviour
{
    [Header("=== Difficulty Scaling ===")]
    [Tooltip("보스를 잡을 때마다 증가하는 비율(0.2면 20% 증가")]
    protected float healthMultiplier = 0.5f;
    protected float damageMultiplier = 0.5f;

    // 입력한 값만큼 체력과 공격력에 배율 적용
    public void SetDifficulty(int difficultyLevel)
    {
        maxHealth *= (int)(healthMultiplier * (float)difficultyLevel);
        currentHealth = maxHealth;
        attackDamage *= (int)(damageMultiplier * (float)difficultyLevel);
    }
    [Header("=== HP ===")]
    public int maxHealth = 1000;
    protected float currentHealth;

    public bool IsAlive => currentHealth > 0;
    protected bool isDead = false;

    [Header("처치시 EXP")]
    public int exp = 100;

    [Header("=== Movement ===")]
    public float moveSpeed = 3f;
    public bool isFlying = false;

    // 플레이어를 인식하는 최대 거리 (일반 몬스터/보스 공통)
    public float detectionRange = 100f;

    //공격 가능한 거리 (일반 몬스터/보스 공통)
    public float attackRange = 10f;

    // true면 Y축은 자기 높이에 고정(일반 몬스터처럼 평면 이동), false면 보스처럼 그대로 LookAt
    [Tooltip("수평면에서만 바라보고 이동할지 여부")]
    public bool lockYToSelf = true;

    [Header("=== Attack ===")]
    public int attackDamage = 10;
    public float attackCooldown = 1f;
    protected float lastAttackTime = 0f;

    [Header("=== Animation & Physics ===")]
    public float deathAnimationDuration = 3f;

    protected Rigidbody rb;
    protected Animator animator;

    [Header("=== Target ===")]
    protected Transform player;
    protected Player playerScript;

    [Header("=== Status Effects ===")]
    protected bool isStunned = false;
    protected float stunTimer = 0f;

    protected float slowMultiplier = 1f; // 1.0 = 정상, 0.5 = 50% 감속
    protected float slowDuration = 0f;

    [Header("=== Debuff Colors ===")]
    [Tooltip("추가 피해 디버프에 걸렸을 때 적용할 색상")]
    [SerializeField] private Color Color_AdditionalDamage = new Color(1f, 0.4f, 0.4f); // 연한 붉은색 기본값

    [Tooltip("빙결(얼어붙음) 디버프에 걸렸을 때 적용할 색상")]
    [SerializeField] private Color Color_Frozen = new Color(0.6f, 0.8f, 1f); // 연한 파란색 기본값

    [Header("=== Debuff: Additional Damage ===")]
    [Tooltip("0.2면 20% 추가 피해")]
    [SerializeField] private float additionalDamageRate = 0f;     // 현재 적용 중인 추가 피해 비율
    private float additionalDamageTimer = 0f;                     // 남은 시간

    // 디버프 색을 적용할 렌더러와 원본 색상
    private Renderer[] debuffRenderers;
    private Color[] originalColors;
    private bool originalColorsCached = false;

    [Header("=== UI ===")]
    [Tooltip("데미지 텍스트가 기준으로 삼을 머리 위치. 비워두면 transform.position 기준.")]
    [SerializeField] private Transform damageTextAnchor;


    [Header("=== CC Immunity ===")]
    [SerializeField] protected bool isCCImmune = false;
    protected float ccImmuneTimer = 0f;



    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = !isFlying;
            rb.isKinematic = false;
            // 쓰러지지 않도록 X/Z 회전 고정
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"{name}: Animator not found in children.");
        }


        CacheDebuffRenderersAndColors();
    }

    // 디버프 색 적용용 렌더러와 원본 색 캐싱
    private void CacheDebuffRenderersAndColors()
    {
        if (originalColorsCached) return;

        if (debuffRenderers == null || debuffRenderers.Length == 0)
        {
            debuffRenderers = GetComponentsInChildren<Renderer>();
        }

        if (debuffRenderers == null || debuffRenderers.Length == 0)
            return;

        originalColors = new Color[debuffRenderers.Length];
        for (int i = 0; i < debuffRenderers.Length; i++)
        {
            if (debuffRenderers[i] == null) continue;
            // material 사용: sharedMaterial 말고 인스턴스 색만 변경
            originalColors[i] = debuffRenderers[i].material.color;
        }

        originalColorsCached = true;
    }

    protected virtual void OnEnable()
    {
        // 재사용(풀링) 대비
        isDead = false;
    }

    protected virtual void Update()
    {
        UpdateStatusEffects();
    }

    /// <summary>
    /// 물리 업데이트 루프.
    /// 파생 클래스에서 FixedUpdate를 오버라이드 하더라도
    /// 반드시 base.FixedUpdate()를 호출해서 공통 처리 유지하도록 설계.
    /// </summary>
    protected virtual void FixedUpdate()
    {
        if (isDead) return;

        // 플레이어가 비활성화되면 참조 초기화
        if (player != null && !player.gameObject.activeInHierarchy)
        {
            player = null;
            playerScript = null;
            SetMoveAnimation(false);
        }

        // 타겟이 없으면 자동으로 한 번 찾아보기
        if (player == null)
        {
            FindPlayer();
        }
    }

    // 0) 플레이어 찾기 (공통)
    virtual protected void FindPlayer()
    {
        if (player != null && player.gameObject.activeInHierarchy) return;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerScript = playerObject.GetComponent<Player>();

            if (playerScript == null)
            {
                Debug.LogError($"{name}: 'Player' tag object has no Player component.");
            }
        }
    }

    // 1) 데미지 처리 
    public virtual void TakeDamage(float dmg, GameObject attacker = null)
    {
        if (isDead) return;

        float finalDamage = dmg * (1f + additionalDamageRate);
        bool isCritical;
        finalDamage *= SkillManager.Instance.GetCritMultiplier(out isCritical);   // 치명타 반영
        currentHealth -= finalDamage;

        // 🔹 데미지 텍스트
        CombatUIManager.Instance?.ShowDamageText(finalDamage, GetDamageTextAnchor(), isCritical);

        // 🔹 에임 포인터에 타격 효과 전달
        CombatUIManager.Instance?.aimPointer.OnDealDamage(finalDamage);

        OnHit(attacker);

        if (currentHealth <= 0)
            Die(attacker);
    }

    // 2) 슬로우 처리
    public virtual void TakeSlow(float multiplier, float duration)
    {
        if (isDead || isCCImmune) return;
        if (multiplier < slowMultiplier) // 더 강한 슬로우만 반영
        {
            slowMultiplier = multiplier;
            slowDuration = duration;
        }
    }

    // 3) 스턴 처리
    public virtual void TakeStun(float duration)
    {
        if (isDead || isCCImmune) return;
        isStunned = true;
        stunTimer = duration;
        ApplyColor(Color_Frozen);
    }

    // 4) DOT(중독 등)
    public virtual void TakeDOT(float dps, float duration)
    {
        StartCoroutine(DOTCoroutine(dps, duration));
    }

    // 5) 추가 피해 디버프
    public virtual void TakeDebuff_AdditionalDamage(float rate, float duration)
    {
        if (rate <= 0f || duration <= 0f) return;
        additionalDamageRate = Mathf.Max(additionalDamageRate, rate);
        additionalDamageTimer = Mathf.Max(additionalDamageTimer, duration);
        ApplyColor(Color_AdditionalDamage);
    }

    private System.Collections.IEnumerator DOTCoroutine(float dps, float duration)
    {
        float timer = 0f;
        float tickInterval = 1f;   // 1초마다 한 번씩 타격

        // duration 동안 1초마다 반복
        while (timer < duration && IsAlive)
        {
            // 1) 데미지 1틱 적용
            TakeDamage(dps);   // ⚠️ 여기서 dps는 "1초마다 들어갈 피해량" 의미로 쓰는 거야

            // 2) 다음 틱까지 대기
            float wait = Mathf.Min(tickInterval, duration - timer);
            yield return new WaitForSeconds(wait);
            timer += wait;
        }
    }


    // ---------------------------
    // 상태 업데이트
    // ---------------------------
    protected void UpdateStatusEffects()
    {
        // 스턴
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
            }
        }

        // 슬로우 지속시간
        if (slowDuration > 0)
        {
            slowDuration -= Time.deltaTime;
            if (slowDuration <= 0)
                slowMultiplier = 1f;
        }
        UpdateAnimatorSpeed();

        // 추가 피해 디버프
        if (additionalDamageTimer > 0f)
        {
            additionalDamageTimer -= Time.deltaTime;
            if (additionalDamageTimer <= 0f)
            {
                additionalDamageTimer = 0f;
                additionalDamageRate = 0f;
            }
        }

        // 아무런 디버프가 없다면 색상 원상 복구
        if (!isStunned && additionalDamageTimer >= 0f)
        {
            RestoreOriginalColor();       
        }
    }

    // ---------------------------
    // 5) 공통 사망 처리
    //  - 팀원 스크립트의 공통 사망 로직(애니메이션+물리정지+콜라이더 비활성+딜레이 삭제)을 통합
    // ---------------------------
    protected virtual void Die(GameObject killer = null)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{name} died.");

        OnDeath(killer);

        // 애니메이션
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // 물리 정지
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }

        // 콜라이더 비활성
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        KillCounter.Instance.AddMonsterKill();
        Debug.Log("몬스터가 쓰러졌습니다.");

        PlayerLevelSystem.Instance.AddExp(exp);
        // 사망 모션 재생 후 삭제
        Destroy(gameObject, deathAnimationDuration);
    }

    protected virtual void OnHit(GameObject attacker) { }
    protected virtual void OnDeath(GameObject killer) { }

    // 타겟을 향해 이동 (슬로우, 스턴, 리지드바디, Y 고정 반영)
    // 파생 클래스에서 AI 루프에서 이 함수를 호출.
    protected void MoveTowards(Vector3 target)
    {
        if (!IsAlive || rb == null) return;

        if (isStunned)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            return;
        }

        Vector3 from = rb.position;
        Vector3 to = target;

        // 🔹 지상 몬스터: 수평(XZ) 이동만, Y는 현재 높이 유지 (중력에 맡김)
        if (!isFlying)
        {
            to.y = from.y;
        }

        // 방향 벡터
        Vector3 dir = to - from;
        if (dir.sqrMagnitude < 0.0001f) return;   // 거의 같은 위치면 패스
        dir.Normalize();

        // 🔹 바라보는 방향 (수평만 사용하고 싶으면 y=0)
        Vector3 lookDir = dir;
        if (lockYToSelf)
            lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(lookDir);

        if (isFlying)
        {
            // 🔹 비행 몬스터: 중력 X, 3D 움직임
            float step = moveSpeed * slowMultiplier * Time.fixedDeltaTime;
            Vector3 nextPos = from + dir * step;
            rb.MovePosition(nextPos);
        }
        else
        {
            // 🔹 지상 몬스터: XZ 속도만 제어, Y는 중력/점프 등 물리로
            Vector3 vel = rb.velocity;
            vel.x = dir.x * moveSpeed * slowMultiplier;
            vel.z = dir.z * moveSpeed * slowMultiplier;
            // vel.y 는 건드리지 않음
            rb.velocity = vel;
        }
    }

    
    // 기절, 둔화 시 애니메이션 재생 속도 조절
    private void UpdateAnimatorSpeed()
    {
        if (animator == null) return;

        if (!IsAlive)
        {
            animator.speed = 1f;
            return;
        }

        if (isStunned)
        {
            animator.speed = 0f;      // 기절 = 애니메이션 멈춤
            return;
        }

        animator.speed = slowMultiplier; // 그 외엔 슬로우 배율로
    }


    // 애니메이터의 "Speed" 파라미터를 0 또는 1로 세팅하는 공통 헬퍼.
    protected void SetMoveAnimation(bool isMoving)
    {
        if (animator == null) return;
        animator.SetFloat("Speed", isMoving ? 1f : 0f);
    }

    // 플레이어가 감지 범위 안에 있는지 체크
    protected bool IsTargetInRange(float range)
    {
        if (player == null) return false;
        return Vector3.Distance(transform.position, player.position) <= range;
    }

    protected bool CanAttack()
    {
        if (!IsAlive || isStunned) return false;

        float m = Mathf.Clamp(slowMultiplier, 0.05f, 10f);
        float effectiveCooldown = attackCooldown / m;

        return Time.time >= lastAttackTime + effectiveCooldown;
    }


    // 가장 기본적인 "플레이어에게 데미지 1회 적용" 공통 처리.
    // 애니메이션 트리거는 파생 클래스에서 담당.
    protected void ApplyBasicAttackToPlayer()
    {
        if (playerScript == null) return;

        playerScript.TakeDamage(attackDamage);
        lastAttackTime = Time.time;
    }

    // 색상 변경 (디버프)
    private void ApplyColor(Color color)
    {
        CacheDebuffRenderersAndColors();
        if (debuffRenderers == null || originalColors == null) return;

        for (int i = 0; i < debuffRenderers.Length; i++)
        {
            var r = debuffRenderers[i];
            if (r == null) continue;

            r.material.color = color;
        }
    }

    // 색상 원상 복구
    private void RestoreOriginalColor()
    {
        if (!originalColorsCached) return;
        if (debuffRenderers == null || originalColors == null) return;
        if (debuffRenderers.Length != originalColors.Length) return;

        for (int i = 0; i < debuffRenderers.Length; i++)
        {
            var r = debuffRenderers[i];
            if (r == null) continue;

            r.material.color = originalColors[i];
        }
    }


    // 데미지 텍스트 기준 위치 반환 (없으면 자기 transform)
    public Transform GetDamageTextAnchor()
    {
        return damageTextAnchor != null ? damageTextAnchor : this.transform;
    }

    // 비행 모드 전환 
    protected void SetFlyingMode(bool flying)
    {
        isFlying = flying;

        if (rb != null)
        {
            rb.useGravity = !isFlying;

            if (isFlying)
            {
                // 중력 끄면서 이상한 낙하 속도 남지 않도록
                rb.velocity = Vector3.zero;
            }
        }
    }

    // CC 해제 + 일정 시간 CC 면역
    // immunityDuration <= 0이면 그냥 CC만 해제하고, 면역은 안 줌
    protected void ClearCrowdControl(float immunityDuration = 0f)
    {
        // 스턴/슬로우 전부 해제
        isStunned = false;
        stunTimer = 0f;

        slowMultiplier = 1f;
        slowDuration = 0f;

        // 애니메이션 속도 원상 복구
        UpdateAnimatorSpeed();

        // 색도 원래대로 (빙결 색 같은 것 복원)
        RestoreOriginalColor();

        // 입력된 시간 동안 CC 면역
        if (immunityDuration > 0f)
        {
            isCCImmune = true;
            ccImmuneTimer = immunityDuration;
        }
    }




}
