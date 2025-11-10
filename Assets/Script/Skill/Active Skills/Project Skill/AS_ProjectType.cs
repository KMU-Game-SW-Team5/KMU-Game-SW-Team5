using System.Collections;
using UnityEngine;

// 가로로 여러 방향과 횟수로 투사체를 발사할 수 있는 스킬
// 플레이 중에 세 가지의 수정이 가능함. 가지의 개수, 연발의 횟수, 투사체 설정
// 투사체의 설정은 현재 미구현 상태.
[CreateAssetMenu(menuName = "Scriptable Object/Active Skills/Project Type")]
public class AS_ProjectType : ActiveSkillBase
{
    [Header("투사체 설정")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private Vector3 acceleration;
    [SerializeField] private bool penetrable = false;
    [SerializeField] private Motion projectileMotion;
    [SerializeField] private float distanceOffset = 10f;

    [Header("발사 패턴")]
    [Min(1)][SerializeField] private int branchCount = 1;    // 가지 개수
    [Min(1)][SerializeField] private int burstCount = 1;     // 연속 발사 개수
    [Range(0f, 90f)][SerializeField] private float maxSpreadAngle = 60f; // 가지 퍼짐 각도 (최대)
    [Range(0f, 1f)][SerializeField] private float minInterval = 0.05f;   // 연속발사 최소 간격
    [Range(0.05f, 1f)][SerializeField] private float maxInterval = 0.4f; // 연속발사 최대 간격
    [SerializeField] private float decayK = 0.4f;                         // 연속 간격 수렴 상수

    // 스킬 발동
    protected override void Execute(GameObject user)
    {
        MonoBehaviour runner = user.GetComponent<MonoBehaviour>();
        if (runner != null)
            runner.StartCoroutine(FireRoutine(user));
        else
            Debug.LogWarning("AS_ProjectTypeV2: MonoBehaviour 실행 주체가 없습니다.");
    }

    // 부채꼴 모양으로 연속 발사
    private IEnumerator FireRoutine(GameObject user)
    {
        // 기본 발사 방향 / 위치 계산
        Vector3 forward = SkillManager.GetForwardDirection();
        Vector3 spawnPos = SkillManager.GetCameraPosition() + forward * distanceOffset;
        Quaternion baseRot = Quaternion.LookRotation(forward);

        // 총 가지 퍼짐 각도 계산 (HorizontalMultiShot 방식)
        float k = 0.25f;
        float dynamicSpread = maxSpreadAngle * (1f - Mathf.Exp(-k * (branchCount - 1)));

        // 가지별 방향 계산 후 각자 코루틴으로 병렬 발사
        for (int i = 0; i < branchCount; i++)
        {
            float t = (branchCount == 1) ? 0f : (float)i / (branchCount - 1);
            float angle = Mathf.Lerp(-dynamicSpread / 2f, dynamicSpread / 2f, t);
            Quaternion shotRot = Quaternion.AngleAxis(angle, Vector3.up) * baseRot;
            Vector3 shotDir = shotRot * Vector3.forward;

            // 각 가지의 연속발사를 동시에 시작
            MonoBehaviour runner = user.GetComponent<MonoBehaviour>();
            if (runner != null)
                runner.StartCoroutine(ShootBurst(user, spawnPos, shotDir));
        }

        // 모든 가지를 동시에 실행하므로 FireRoutine 자체는 즉시 종료
        yield break;
    }


    private IEnumerator ShootBurst(GameObject user, Vector3 spawnPos, Vector3 dir)
    {
        // 연속발사 간격 계산 
        float interval = (burstCount <= 1)
            ? 0f
            : minInterval + (maxInterval - minInterval) * Mathf.Exp(-decayK * (burstCount - 2));

        Quaternion rot = Quaternion.LookRotation(dir);

        for (int n = 0; n < burstCount; n++)
        {
            GameObject projectile = Object.Instantiate(projectilePrefab, spawnPos, rot);

            ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
            pc.SetDestroyComponent(lifeTime, penetrable);
            pc.SetMotionType(projectileMotion);
            pc.SetPhysicalComponent(dir * projectileSpeed, acceleration, null);

            if (interval > 0f && n < burstCount - 1)
                yield return new WaitForSeconds(interval);
        }
    }

    // ================================
    // 가지/연발 수 증감 메서드
    // ================================
    public void IncreaseBranchCount(int n = 1) => branchCount += n;
    public void DecreaseBranchCount(int n = 1) => branchCount = Mathf.Max(1, branchCount - n);

    public void IncreaseBurstCount(int n = 1) => burstCount += n;
    public void DecreaseBurstCount(int n = 1) => burstCount = Mathf.Max(1, burstCount - n);

    // Getter (필요시)
    public int GetBranchCount() => branchCount;
    public int GetBurstCount() => burstCount;
}
