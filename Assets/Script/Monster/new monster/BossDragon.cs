using UnityEngine;
using System.Collections;

public class BossDragon : BossMonsterBase
{
    [Header("Phase 2 Flight")]
    [SerializeField] private float phase2RiseHeight = 10f;   // 2페에서 올라갈 높이
    [SerializeField] private float phase2RiseDuration = 2f;  // 상승에 걸리는 시간(초)

    private Coroutine phase2FlightCoroutine;
    private float phase2FlightY;     // 2페이즈에서 유지할 고도

    [Header("Boss Death Fall")]
    [SerializeField] private float deathFallSpeed = 30f;        // 초기 낙하 속도
    [SerializeField] private float deathGravityMultiplier = 3f; // 기본 중력의 몇 배로 더 끌어당길지
    private Coroutine deathGravityCo;

    [Header("Boss Death Rotation")]
    [SerializeField] private Vector3 deathTorqueAxis = new Vector3(1f, 0f, 0f);  // 회전 축 (방향)
    [SerializeField] private float deathTorqueMagnitude = 50f;                   // 회전 강도
    [SerializeField] private float groundCheckDistance = 3f;                     // “공중인가?” 판정용 아래 체크 거리


    [Header("Landing Effect")]
    [SerializeField] private GameObject landingDustPrefab;     // 먼지 파티클 프리팹
    [SerializeField] private Transform landingEffectPoint;     // 발 위치 기준 포인트
    [SerializeField] private LayerMask groundLayer;            // 땅 레이어
    [SerializeField] private float landingShakeIntensity = 1f; // 카메라 흔들림 세기
    [SerializeField] private float landingShakeDuration = 0.3f;// 카메라 흔들림 시간
    private bool hasLandedAfterDeath = false;

    // 공격 패턴
    protected override void TryAttack()
    {
        if (!CanAttack() || playerScript == null)
            return;

        transform.LookAt(player.position);

        if (animator == null)
            return;

        animator.SetTrigger("BasicAttack");
        playerScript.TakeDamage(attackDamage);

        lastAttackTime = Time.time;
    }

    protected override void OnHit(GameObject attacker)
    {
        if (animator != null)
            animator.SetTrigger("Hit");
    }

    protected override void OnDeath(GameObject killer)
    {
        base.OnDeath(killer);
    }

    // 2페 진입 시: 비행 모드 + 상승
    protected override void EnterPhase2()
    {
        base.EnterPhase2();

        attackRange = phase2AttackRange;

        if (animator != null)
        {
            animator.SetBool("isPhase2", true);
            animator.SetTrigger("startPhase2");
        }

        Debug.Log($"{name}: Enter Phase 2");

        SetFlyingMode(true);
        ClearCrowdControl(phase2RiseDuration);

        if (phase2FlightCoroutine != null)
            StopCoroutine(phase2FlightCoroutine);

        phase2FlightCoroutine = StartCoroutine(Phase2FlyUpRoutine());
    }

    private IEnumerator Phase2FlyUpRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * phase2RiseHeight;

        float elapsed = 0f;

        // 죽으면 바로 루프 탈출
        while (!isDead && elapsed < phase2RiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / phase2RiseDuration);

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        // 살아 있는 상태에서만 타겟 위치까지 보정
        if (!isDead)
        {
            transform.position = targetPos;
            phase2FlightY = targetPos.y;
        }
        else
        {
            // 중간에 죽었다면, 그때의 높이를 기준 고도로 사용
            phase2FlightY = transform.position.y;
        }

        phase2FlightCoroutine = null;
    }

    // 2페이즈에서는 현재 고도 유지한 채 수평 이동
    protected override void MoveBossTowards(Vector3 target)
    {
        if (hasEnteredPhase2)
        {
            target.y = phase2FlightY;
            MoveTowards(target);
        }
        else
        {
            base.MoveBossTowards(target);
        }
    }

    protected override void Die(GameObject killer = null)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{name} died.");

        // 2페이즈 상승 코루틴이 돌고 있으면 강제 종료
        if (phase2FlightCoroutine != null)
        {
            StopCoroutine(phase2FlightCoroutine);
            phase2FlightCoroutine = null;
        }

        OnDeath(killer);
        SetFlyingMode(false);

        if (animator != null)
        {
            // 혹시 전환 트리거가 걸린 상태면 리셋해두는 것도 안전
            animator.ResetTrigger("startPhase2");
            animator.SetBool("isPhase2", false);
            animator.SetTrigger("Die");
        }

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;

            rb.velocity = Vector3.zero;
            rb.drag = 0f;
            rb.angularDrag = 0.05f;

            rb.freezeRotation = false;

            // ★ 공중에서 죽었으면 회전 토크 추가
            if (!IsNearGround())
            {
                ApplyDeathRotationTorque();
            }

            // 추가 중력 코루틴 시작
            if (deathGravityCo != null)
                StopCoroutine(deathGravityCo);
            deathGravityCo = StartCoroutine(DeathExtraGravityRoutine());
        }

        Destroy(gameObject, deathAnimationDuration + 10f);
    }

    private bool IsNearGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        bool hit = Physics.Raycast(ray, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);
        return hit;
    }

    private void ApplyDeathRotationTorque()
    {
        if (rb == null) return;
        Vector3 localAxis = deathTorqueAxis.sqrMagnitude > 0.0001f
            ? deathTorqueAxis.normalized
            : Vector3.right; // 기본값: 로컬 X축(앞으로 고꾸라지는 피치 회전에 쓰기 좋음)

        // 로컬 축을 월드 축으로 변환
        Vector3 worldAxis = transform.TransformDirection(localAxis);

        Vector3 torque = worldAxis * deathTorqueMagnitude;
        rb.AddTorque(torque, ForceMode.VelocityChange);
    }



    private IEnumerator DeathExtraGravityRoutine()
    {
        float extraMultiplier = Mathf.Max(1f, deathGravityMultiplier);

        while (isDead && rb != null)
        {
            Vector3 extraGravity = Physics.gravity * (extraMultiplier - 1f);
            rb.AddForce(extraGravity, ForceMode.Acceleration);

            extraMultiplier *= 1.02f;
            yield return new WaitForFixedUpdate();
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!isDead || hasLandedAfterDeath) return;

        if ((groundLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        hasLandedAfterDeath = true;

        PlayLandingDust();
        PlayLandingCameraShake();
    }

    private void PlayLandingDust()
    {
        if (landingDustPrefab == null) return;

        Vector3 pos = landingEffectPoint != null
            ? landingEffectPoint.position
            : transform.position;

        Quaternion rot = Quaternion.Euler(-90f, transform.eulerAngles.y, 0f);
        GameObject go = GameObject.Instantiate(landingDustPrefab, pos, rot);
    }

    private void PlayLandingCameraShake()
    {
        CombatUIManager.Instance?.PlayCameraShake(landingShakeIntensity, landingShakeDuration);
    }
}
