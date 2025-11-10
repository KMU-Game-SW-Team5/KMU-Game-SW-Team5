using UnityEngine;

// 가로로 여러개의 투사체를 발사하는 방식

[System.Serializable]
public class HorizontalMultiShot : IShotType
{
    [Range(1, 20)] public int projectileCount = 5;  // 투사체의 개수
    [Range(0, 90)] public float spreadAngle = 30f;  // 투사 각도
    private Motion projectileMotion;                // 투사체에 적용할 운동 방식
    public void ProjectileMotionChange(Motion motion) {  projectileMotion = motion; } // 운동 방식 변경

    public void Shoot(GameObject user, AS_ProjectType skill)
    {
        float damage = skill.GetPower(skill.magicStat);

        Vector3 lookingDir = SkillManager.GetForwardDirection();
        Vector3 spawnPos = user.transform.position + user.transform.TransformDirection(lookingDir * skill.distanceOffset);

        // 부채꼴 발사
        for (int i = 0; i < projectileCount; i++)
        {
            float t = (projectileCount == 1) ? 0f : (float)i / (projectileCount - 1);
            float angle = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);

            // 부채꼴 각도 설정
            Quaternion baseRot = Quaternion.LookRotation(lookingDir);
            Quaternion shotRot = Quaternion.AngleAxis(angle, Vector3.up) * baseRot;

            // 투사체 생성
            GameObject projectile = Object.Instantiate(skill.projectilePrefab, spawnPos, shotRot);

            // 투사체의 속성 설정
            ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
            pc.SetDestroyComponent(skill.lifeTime, skill.penetrable);
            pc.velocity = (shotRot * Vector3.forward) * skill.projectileSpeed;  // 쿼터니언 * 벡터 = 방향 벡터
            pc.acceleration = skill.acceleration;
            pc.SetMotionType(skill.projectileMotion);
        }
    }
}
