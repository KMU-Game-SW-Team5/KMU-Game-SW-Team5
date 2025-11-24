using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
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

    void Start()
    {
        hp = maxHealth;
        moveController = GetComponent<MoveController>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    public void TakeDamage(int damage)
    {

        if (isInvincible) return; // 무적 상태라면 데미지 입지 않음.

        hp -= damage;
        Debug.Log("플레이어 체력: " + hp);

        CombatUIManager.Instance?.PlayHitEffect();
        LowHPEffect();
        playerAnimation.SetAnimation(AnimationType.Hit);

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

    void Heal(int heal)
    {
        hp = Mathf.Max(hp + heal, maxHealth);
        LowHPEffect();
    }

    void Die()
    {
        Debug.Log("플레이어가 쓰러졌습니다.");
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