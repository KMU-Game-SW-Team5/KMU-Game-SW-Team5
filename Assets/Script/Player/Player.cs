using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int hp = 100;
    [SerializeField] private float lowHpRatio = 0.2f;
    public float hpRatio => (float)hp/(float)maxHealth; 

    private MoveController moveController;
    private PlayerAnimation playerAnimation;

    [Header("무적 상태 관련")]
    private bool isInvincible = false;
    private Coroutine invincibleCo;
    [SerializeField] private float invincibleTimeWhenTakeDamage = 1f;

    // HP 변화 이벤트 -> UI 연결
    public event Action<int, int> OnHPChanged;

    void Start()
    {
        hp = maxHealth;
        moveController = GetComponent<MoveController>();
        playerAnimation = GetComponent<PlayerAnimation>();

        OnHPChanged?.Invoke(hp, maxHealth);
    }

    public void TakeDamage(int damage)
    {

        if (isInvincible) return; // 무적 상태라면 데미지 입지 않음.

        hp -= damage;
        Debug.Log("플레이어 체력: " + hp);

        CombatUIManager.Instance?.PlayHitEffect();
        LowHPEffect();
        playerAnimation.SetAnimation(AnimationType.Hit);
        OnHPChanged?.Invoke(hp, maxHealth);

        SetInvincibleFor(invincibleTimeWhenTakeDamage);

        if (hp <= 0)
        {
            Die();
        }
    }

    public void LowHPEffect()
    {
        if (hpRatio < lowHpRatio)
        {
            CombatUIManager.Instance?.StartLowHpEffect();
        }
        else
        {
            CombatUIManager.Instance?.StopLowHpEffect();
        }
    }

    public void Heal(int value)
    {
        hp = Mathf.Min(hp + value, maxHealth);
        Debug.Log("Healed, currentHP : " + hp);
        LowHPEffect();
    }

    public void IncreaseMaxHealth(int value)
    {
        maxHealth += value;
        Heal(value);
    }

    void Die()
    {
        Debug.Log("플레이어가 쓰러졌습니다.");
        GameManager.Instance.EndGame(false);
        Time.timeScale = 0f;
    }

    public void SetInvincibleFor(float duration)
    {
        if (invincibleCo != null)
            StopCoroutine(invincibleCo);
        invincibleCo = StartCoroutine(InvincibleRoutine(duration));
    }

    private IEnumerator InvincibleRoutine(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
        invincibleCo = null;
    }

}