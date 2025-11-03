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
    [SerializeField] public float verticalAccel = 0f;      // 수직 방향 움직임(양수면 부상, 음수면 추락)
    [SerializeField] public bool penetrable = false;       // 관통성
    [SerializeField] public Vector3 instantiateOffset;     // 투사체를 생성하는 위치 오프셋(Z축 방향이 정면)

    [Header("발사 방식")]
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

    //// 추상화된 레거시 코드
    //public void generalShot(GameObject user)
    //{
    //    float damage = GetPower(magicStat);      // 스킬의 위력 설정

    //    // TODO : 하단의 Camera.main으로 호출하는 방식은 비효율적임. 기회가 되면 최적화 할 것.

    //    // 투사체 생성 위치와 회전값 설정
    //    Vector3 spawnPos = user.transform.position + user.transform.TransformDirection(instantiateOffset);
    //    Quaternion lookingRotation = Quaternion.LookRotation(Camera.main.transform.forward); // 카메라 보는 방향 기준 

    //    // 투사체 생성
    //    GameObject projectile = Instantiate(projectilePrefab, spawnPos, lookingRotation);

    //    // 투사체의 속성 설정
    //    ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
    //    pc.SetComponent(damage, lifeTime, penetrable, Camera.main.transform.forward * projectileSpeed, verticalAccel);
    //}

}
