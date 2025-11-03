using UnityEngine;

[System.Serializable]
public class HorizontalMultiShot : IShotType
{
    // 흩뿌리는 타입
    [Range(1, 20)] public int projectileCount = 5;
    [Range(0, 90)] public float spreadAngle = 30f;

    public void Shoot(GameObject user, AS_ProjectType skill)
    {
        float damage = skill.GetPower(skill.magicStat);

        // LinearShot과 동일한 기준 위치 사용
        Vector3 spawnPos = user.transform.position + user.transform.TransformDirection(skill.instantiateOffset);

        // 기준 방향 (카메라 바라보는 방향)
        Vector3 forward = Camera.main.transform.forward;

        // 부채꼴 발사
        for (int i = 0; i < projectileCount; i++)
        {
            float t = (projectileCount == 1) ? 0f : (float)i / (projectileCount - 1);
            float angle = Mathf.Lerp(-spreadAngle / 2f, spreadAngle / 2f, t);

            // 부채꼴 각도 설정
            Quaternion baseRot = Quaternion.LookRotation(forward);
            Quaternion shotRot = Quaternion.AngleAxis(angle, Vector3.up) * baseRot;

            // 투사체 생성
            GameObject projectile = Object.Instantiate(skill.projectilePrefab, spawnPos, shotRot);

            // 투사체 속성 설정
            if (projectile.TryGetComponent(out ProjectileComponent pc))
            {
                pc.SetComponent(
                    damage,
                    skill.lifeTime,
                    skill.penetrable,
                    shotRot * Vector3.forward * skill.projectileSpeed,
                    skill.verticalAccel
                );
            }
        }
    }
}
