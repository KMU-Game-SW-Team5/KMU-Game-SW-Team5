using System.Collections;
using UnityEngine;

// 가로 방향 + 연속 발사가 가능한 투사체 스킬
// - branchCount : 가지 개수 (부채꼴 분포)
// - burstCount  : 연속 발사 횟수
// - projectileMotion : 운동 로직 (ScriptableObject 기반)
[CreateAssetMenu(menuName = "Scriptable Object/Active Skills/Project Type")]
public class AS_ProjectType : ActiveSkillBase
{
    [Header("투사체 설정")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 20f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private bool penetrable = false;
    [SerializeField] private Motion projectileMotion;
    [SerializeField] private float motionSpeed = 1.0f;
    [SerializeField] private float distanceOffset = 10f;

    [Header("발사 패턴")]
    [Min(1)][SerializeField] private int branchCount = 1;
    [Min(1)][SerializeField] private int burstCount = 1;
    [Range(0f, 90f)][SerializeField] private float maxSpreadAngle = 60f;
    [Range(0f, 1f)][SerializeField] private float minInterval = 0.05f;
    [Range(0.05f, 1f)][SerializeField] private float maxInterval = 0.4f;
    [SerializeField] private float decayK = 0.4f;

    private Transform target;

    protected override void Execute(GameObject user, Transform _target)
    {
        this.target = _target;

        // 🔸 코루틴 실행 주체를 명확히 SkillManager로 고정
        SkillManager runner = user.GetComponent<SkillManager>();
        if (runner != null)
            runner.StartCoroutine(FireRoutine(user));
        else
            Debug.LogWarning("AS_ProjectType: SkillManager를 찾을 수 없습니다.");
    }

    private IEnumerator FireRoutine(GameObject user)
    {
        Vector3 forward;
        Vector3 spawnPos;
        Quaternion baseRot;

        float k = 0.25f;
        float dynamicSpread = maxSpreadAngle * (1f - Mathf.Exp(-k * (branchCount - 1)));
        float interval = (burstCount <= 1)
            ? 0f
            : minInterval + (maxInterval - minInterval) * Mathf.Exp(-decayK * (burstCount - 2));

        // n 번째 발사
        for (int n = 0; n < burstCount; n++)
        {
            // 발사 때마다 플레이어 위치, 바라본 방향 다시 체크
            forward = SkillManager.GetForwardDirection();
            spawnPos = SkillManager.GetCameraPosition() + forward * distanceOffset;
            baseRot = Quaternion.LookRotation(forward);
            // 부채꼴 발사
            for (int i = 0; i < branchCount; i++)
            {
                float t = (branchCount == 1) ? 0f : (float)i / (branchCount - 1);
                float angle = Mathf.Lerp(-dynamicSpread / 2f, dynamicSpread / 2f, t);
                Quaternion shotRot = Quaternion.AngleAxis(angle, Vector3.up) * baseRot;
                Vector3 shotDir = shotRot * Vector3.forward;

                GameObject projectile = ObjectPooler.Instance.Spawn(projectilePrefab, spawnPos, shotRot);

                ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
                pc.SetPrefabRef(projectilePrefab);
                pc.SetDestroyComponent(lifeTime, penetrable);
                pc.SetMotionType(projectileMotion);
                pc.SetPhysicalComponent(target, shotDir * projectileSpeed, motionSpeed);
            }

            // 🔸 모든 branch가 전부 나간 다음에만 딜레이 시작
            if (interval > 0f && n < burstCount - 1)
                yield return new WaitForSeconds(interval);
        }
    }


    // ========== 패턴 조절 메서드 ==========
    public void IncreaseBranchCount(int n = 1) => branchCount += n;
    public void DecreaseBranchCount(int n = 1) => branchCount = Mathf.Max(1, branchCount - n);

    public void IncreaseBurstCount(int n = 1) => burstCount += n;
    public void DecreaseBurstCount(int n = 1) => burstCount = Mathf.Max(1, burstCount - n);

    public int GetBranchCount() => branchCount;
    public int GetBurstCount() => burstCount;
}
