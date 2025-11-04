using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxHealth = 100;
    public int hp = 100;

    private MoveController moveController;

    void Start()
    {
        hp = maxHealth;
        moveController = GetComponent<MoveController>();
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
        Debug.Log("플레이어 체력: " + hp);

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