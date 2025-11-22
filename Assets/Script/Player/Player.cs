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

    void Start()
    {
        hp = maxHealth;
        moveController = GetComponent<MoveController>();
        playerAnimation = GetComponent<PlayerAnimation>();
    }

    public void TakeDamage(int damage)
    {
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

}