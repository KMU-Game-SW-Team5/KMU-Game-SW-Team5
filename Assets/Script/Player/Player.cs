using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int hp = 100;

    private MoveController moveController;

    // HP 변화 이벤트 -> UI 연결
    public event Action<int, int> OnHPChanged;

    void Start()
    {
        hp = maxHealth;
        moveController = GetComponent<MoveController>();

        OnHPChanged?.Invoke(hp, maxHealth);
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
        GameManager.Instance.EndGame(false);
        Time.timeScale = 0f;
    }

    void Update()
    {
        
    }
}