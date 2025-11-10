using UnityEngine;

// 스킬 투사 방식을 추상화한 인터페이스
public interface IShotType
{
    void Shoot(GameObject user, AS_ProjectType skill);

    void IncreaseProjectile(int n = 1);
    void DecreaseProjectile(int n = 1);
}
