using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(menuName ="Skills/Active/Fireball Skill")]
// 투사체 발사형 스킬들의 컴포넌트
public class AS_ProjectType : ActiveSkillBase
{
    [Header("투사체 설정")]
    [SerializeField] private GameObject projectilePrefab;   // 투사체 프리팹
    [SerializeField] private float projectileSpeed = 1f;    // 투사체의 속도
    [SerializeField] private float lifeTime = 5f;           // 투사체의 수명
    [SerializeField] private float verticalAccel = 0f;      // 수직 방향 움직임(양수면 부상, 음수면 추락)
    [SerializeField] private bool penetrable = false;       // 관통성
    [SerializeField] private Vector3 instantiateOffset;  // 투사체를 생성하는 위치 오프셋

    // 쏘는 방식
    private enum shotType
    {
        general         // 보는 방향으로 일관적으로 발사
    }
    private shotType currentShotType = shotType.general;        // 현재 쏘는 방식

    // 시전 시 호출
    protected override void Execute(GameObject user)
    {
        if (currentShotType == shotType.general)
        {
            generalShot(user);
        }
    }

    // 평범하게 직선으로 쏠 때
    private void generalShot(GameObject user)
    {
        float damage = GetPower(magicStat);      // 스킬의 위력 설정

        // TODO : 하단의 Camera.main으로 호출하는 방식은 비효율적임. 기회가 되면 최적화 할 것.

        // 투사체 생성 위치와 회전값 설정
        Vector3 spawnPos = user.transform.position + user.transform.TransformDirection(instantiateOffset);
        Quaternion lookingRotation = Quaternion.LookRotation(Camera.main.transform.forward); // 카메라 보는 방향 기준 

        // 투사체 생성
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, lookingRotation);

        // 투사체의 속성 설정
        ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
        pc.SetComponent(damage, lifeTime, penetrable, Camera.main.transform.forward * projectileSpeed, verticalAccel);
    }

}
