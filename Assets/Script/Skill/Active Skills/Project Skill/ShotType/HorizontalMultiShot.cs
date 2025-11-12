using UnityEngine;

// 가로로 여러개의 투사체를 발사하는 방식
[System.Serializable]
public class HorizontalMultiShot : IShotType
{
    [Range(1, 20)] public int projectileCount = 5;   // 투사체의 개수
    [Range(0, 90)] public float maxSpreadAngle = 60f; // 최대 총각 (양옆 30도)
    private Motion projectileMotion;                  // 투사체에 적용할 운동 방식
    public void ProjectileMotionChange(Motion motion) => projectileMotion = motion;

    public void Shoot(GameObject user, AS_ProjectTypeLegacy skill)
    {
        float damage = skill.GetPower(skill.magicStat);

        Vector3 lookingDir = SkillManager.GetForwardDirection();
        Vector3 spawnPos = SkillManager.GetCameraPosition() + lookingDir * skill.distanceOffset;

        // 투사체가 많아질 수록 각도가 벌어지고, 최대 60도를 넘지 않음.
        float k = 0.25f;
        float dynamicSpread = maxSpreadAngle * (1f - Mathf.Exp(-k * (projectileCount - 1)));
        // 예: 1개 → 0°, 2개 → 약 13°, 3개 → 약 23°, 5개 → 약 36°, 10개 → 약 49°, 20개 → 57°

        // 부채꼴로 발사
        for (int i = 0; i < projectileCount; i++)
        {
            float t = (projectileCount == 1) ? 0f : (float)i / (projectileCount - 1);
            float angle = Mathf.Lerp(-dynamicSpread / 2f, dynamicSpread / 2f, t);

            Quaternion baseRot = Quaternion.LookRotation(lookingDir);
            Quaternion shotRot = Quaternion.AngleAxis(angle, Vector3.up) * baseRot;

            GameObject projectile = Object.Instantiate(skill.projectilePrefab, spawnPos, shotRot);

            ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
            pc.SetDestroyComponent(skill.lifeTime, skill.penetrable);
            //pc.velocity = (shotRot * Vector3.forward) * skill.projectileSpeed;
            //pc.acceleration = skill.acceleration;
            pc.SetMotionType(skill.projectileMotion);
        }
    }

    public void IncreaseProjectile(int n = 1) => projectileCount += n;
    public void DecreaseProjectile(int n = 1) => projectileCount = Mathf.Max(1, projectileCount - n);
}
