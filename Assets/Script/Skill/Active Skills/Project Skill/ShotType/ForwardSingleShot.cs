using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ForwardSingleShot : IShotType
{
    // 직선으로 한 개 발사하는 타입
    public void Shoot(GameObject user, AS_ProjectType skill)
    {
        float damage = skill.GetPower(skill.magicStat);      // 스킬의 위력 설정

        // TODO : 하단의 Camera.main으로 호출하는 방식은 비효율적임. 기회가 되면 최적화 할 것.

        // 투사체 생성 위치와 회전값 설정
        Vector3 spawnDir = SkillManager.GetForwardDirection();
        Vector3 spawnPos = SkillManager.GetCameraPosition() + spawnDir * skill.distanceOffset;
        Quaternion lookingRotation = Quaternion.LookRotation(spawnDir); // 카메라 보는 방향 기준

        // 투사체 생성
        GameObject projectile = Object.Instantiate(skill.projectilePrefab, spawnPos, lookingRotation);

        // 투사체의 속성 설정
        ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
        pc.SetDestroyComponent(skill.lifeTime, skill.penetrable);
        pc.velocity = SkillManager.GetForwardDirection() * skill.projectileSpeed;
        pc.acceleration = skill.acceleration;
        pc.SetMotionType(skill.projectileMotion);
    }
}
