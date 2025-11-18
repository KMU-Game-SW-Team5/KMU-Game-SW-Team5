using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// IDamageable.cs
public interface IDamageable
{
    // "이 인터페이스를 따르는 모든 스크립트는 
    // TakeDamage(int damage) 함수를 반드시 가지고 있어야 한다"는 계약입니다.
    void TakeDamage(int damage);
}