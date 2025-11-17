using UnityEngine;

public abstract class MonsterBase : MonoBehaviour
{
    [Header("=== HP ===")]
    public float maxHP = 100;
    protected float currentHP;

    public bool IsAlive => currentHP > 0;
    protected bool isDead = false;

    [Header("=== Movement ===")]
    public float moveSpeed = 3f;

    [Header("=== Status Effects ===")]
    protected bool isStunned = false;
    protected float stunTimer = 0f;

    protected float slowMultiplier = 1f; // 1.0 = 정상, 0.5 = 50% 감속
    protected float slowDuration = 0f;

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    protected virtual void Update()
    {
        UpdateStatusEffects();
    }

    // ---------------------------
    // 1) 데미지 처리
    // ---------------------------
    public virtual void TakeDamage(float dmg, GameObject attacker = null)
    {
        if (isDead) return;

        currentHP -= dmg;
        Debug.Log($"{name} took {dmg} damage");
        Debug.Log($"{name} 's current HP : {currentHP}");

        OnHit(attacker);

        if (currentHP <= 0)
            Die(attacker);
    }

    // ---------------------------
    // 2) 슬로우 처리
    // ---------------------------
    public virtual void TakeSlow(float multiplier, float duration)
    {
        if (multiplier < slowMultiplier) // 더 강한 슬로우만 반영
        {
            slowMultiplier = multiplier;
            slowDuration = duration;
        }
    }

    // ---------------------------
    // 3) 스턴 처리
    // ---------------------------
    public virtual void TakeStun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
    }

    // ---------------------------
    // 4) DOT(중독 등)
    // ---------------------------
    public virtual void TakeDOT(float dps, float duration)
    {
        StartCoroutine(DOTCoroutine(dps, duration));
    }

    private System.Collections.IEnumerator DOTCoroutine(float dps, float duration)
    {
        float timer = 0f;
        while (timer < duration && IsAlive)
        {
            TakeDamage(dps * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
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
                isStunned = false;
        }

        // 슬로우 지속시간
        if (slowDuration > 0)
        {
            slowDuration -= Time.deltaTime;
            if (slowDuration <= 0)
                slowMultiplier = 1f;
        }
    }

    // ---------------------------
    // 5) 공통 사망 처리
    // ---------------------------
    protected virtual void Die(GameObject killer = null)
    {
        if (isDead) return;

        isDead = true;
        Debug.Log($"{name} died.");

        OnDeath(killer);

        Destroy(gameObject, 0.1f);
    }

    // ---------------------------
    // 6) 상속 후 덮어쓰는 Hook들
    // ---------------------------
    protected virtual void OnHit(GameObject attacker) { }
    protected virtual void OnDeath(GameObject killer) { }

    // ---------------------------
    // 7) 이동 함수 (슬로우 반영)
    // ---------------------------
    protected void MoveTowards(Vector3 target)
    {
        if (!IsAlive || isStunned) return;

        Vector3 dir = (target - transform.position).normalized;
        transform.position += dir * moveSpeed * slowMultiplier * Time.deltaTime;
    }
}
