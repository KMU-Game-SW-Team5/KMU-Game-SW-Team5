using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LinearShot : IShotType
{
    // 직선으로 한 개 발사하는 타입
    public void Shoot(GameObject user, AS_ProjectType skill)
    {
        float damage = skill.GetPower(skill.magicStat);      // 스킬의 위력 설정

        // TODO : 하단의 Camera.main으로 호출하는 방식은 비효율적임. 기회가 되면 최적화 할 것.

        // 투사체 생성 위치와 회전값 설정
        Vector3 spawnPos = user.transform.position + user.transform.TransformDirection(skill.instantiateOffset);
        Quaternion lookingRotation = Quaternion.LookRotation(Camera.main.transform.forward); // 카메라 보는 방향 기준 

        // 투사체 생성
        GameObject projectile = Object.Instantiate(skill.projectilePrefab, spawnPos, lookingRotation);

        // 투사체의 속성 설정
        ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
        pc.SetComponent(damage, skill.lifeTime, skill.penetrable, 
            Camera.main.transform.forward * skill.projectileSpeed, skill.verticalAccel);
    }
}
