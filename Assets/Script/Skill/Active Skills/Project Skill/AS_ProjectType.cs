using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName ="Scriptable Object/Active Skills/Project Type")]
// 투사체 발사형 스킬들의 컴포넌트
public class AS_ProjectType : ActiveSkillBase
{
    [Header("투사체 설정")]
    [SerializeField] public GameObject projectilePrefab;   // 투사체 프리팹
    [SerializeField] public float projectileSpeed = 1f;    // 투사체의 속도
    [SerializeField] public float lifeTime = 5f;           // 투사체의 수명
    [SerializeField] public Vector3 acceleration;          // 가속도 벡터
    [SerializeField] public bool penetrable = false;       // 관통성
    [SerializeField] public Motion projectileMotion;       // 투사체의 운동 방식
    [SerializeField] public float distanceOffset = 10f;    // 투사체가 생성되는 전방 거리 오프셋

    [Header("투사 방식")]
    [SerializeReference] private IShotType shotType;
    public IShotType GetShotType() => shotType;

    // 발사 방식을 교체하는 함수
    public void SetShotType(IShotType newShotType)
    {
        if (newShotType != null)
        {
            shotType = newShotType;
        }
    }

    // 시전 시 호출
    protected override void Execute(GameObject user)
    {
        shotType?.Shoot(user, this);
    }

    // 탄환 개수 증가
    public void IncreaseProjectileNum(int n = 1)
    {
        shotType?.IncreaseProjectile(n);
    }

    // 탄환 개수 감소. 1 이하로는 떨어지지 않음.
    public void DecreaseProjectileNum(int n = 1)
    {
        shotType?.DecreaseProjectile(n);
    }
}
