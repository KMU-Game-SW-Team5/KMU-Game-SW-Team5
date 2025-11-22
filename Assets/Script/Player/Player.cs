using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth = 100;
    public int hp = 100;

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

        playerAnimation.SetAnimation(AnimationType.Hit);


        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("플레이어가 쓰러졌습니다.");
        Time.timeScale = 0f;
    }

    void Update()
    {
        
    }
}