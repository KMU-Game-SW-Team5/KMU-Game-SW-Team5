using System.Collections;
using UnityEngine;

[System.Serializable]
public class ForwardSingleShot : IShotType
{
    [Range(1, 20)] public int projectileCount = 1;       // 발사 개수
    [Range(0.1f, 5f)] public float maxCastTime = 1.0f;   // 최대 시전 시간 (초)
    [Range(0.01f, 1f)] public float minInterval = 0.02f;    // 발사 최소 간격
    [Range(0.05f, 1f)] public float maxInterval = 0.40f;    // 발사 최대 간격 (2개)
    [Range(0.05f, 1f)] public float decayK = 0.4f;         // 수렴 속도 상수
    private Motion projectileMotion;                     // 투사체의 운동 방식
    public void ProjectileMotionChange(Motion motion) => projectileMotion = motion;

    public void Shoot(GameObject user, AS_ProjectTypeLegacy skill)
    {
        // 코루틴을 이용해 순차 발사
        MonoBehaviour runner = user.GetComponent<MonoBehaviour>();
        if (runner != null)
            runner.StartCoroutine(ShootSequence(user, skill));
        else
            Debug.LogWarning("ForwardSingleShot: 코루틴을 실행할 MonoBehaviour를 찾을 수 없습니다.");
    }

    // 정해진 시간 안에 특정 개수를 등간격으로 발사함.
    private IEnumerator ShootSequence(GameObject user, AS_ProjectTypeLegacy skill)
    {
        float damage = skill.GetPower(skill.magicStat);
        Vector3 spawnDir = SkillManager.GetForwardDirection();
        Vector3 spawnPos = SkillManager.GetCameraPosition() + spawnDir * skill.distanceOffset;
        Quaternion lookingRotation = Quaternion.LookRotation(spawnDir);

        // 발사 간격 계산
        float interval = (projectileCount <= 1) ? 0f : 
            interval = minInterval + (maxInterval - minInterval) * Mathf.Exp(-decayK * (projectileCount - 2)); ;

        // 연속으로 발사
        for (int i = 0; i < projectileCount; i++)
        {
            GameObject projectile = Object.Instantiate(skill.projectilePrefab, spawnPos, lookingRotation);

            ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
            pc.SetDestroyComponent(skill.lifeTime, skill.penetrable);
            pc.velocity = spawnDir * skill.projectileSpeed;
            pc.acceleration = skill.acceleration;
            pc.SetMotionType(skill.projectileMotion);

            // 다음 발사까지 대기
            if (interval > 0f && i < projectileCount - 1)
                yield return new WaitForSeconds(interval);
        }
    }

    public void IncreaseProjectile(int n = 1)
    {
        projectileCount += n;
    }

    public void DecreaseProjectile(int n = 1)
    {
        projectileCount = Mathf.Max(1, projectileCount - n);
    }
}
