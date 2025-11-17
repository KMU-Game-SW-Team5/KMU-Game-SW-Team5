using System.Collections;
using UnityEngine;

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

    // ============================================================
    // 스킬 사용 실행
    // ============================================================
    protected override void Execute(GameObject user, Transform _target)
    {
        this.target = _target;

        // SkillManager를 반드시 찾는다 (싱글톤 기반)
        SkillManager runner = user.GetComponent<SkillManager>();
        if (runner != null)
            runner.StartCoroutine(FireRoutine());
        else
            Debug.LogWarning("AS_ProjectType: SkillManager를 찾을 수 없습니다.");
    }

    // ============================================================
    // 실제 투사체 발사 루틴
    // ============================================================
    private IEnumerator FireRoutine()
    {
        Debug.Log(1);
        Vector3 forward;
        Vector3 spawnPos;
        Quaternion baseRot;

        float k = 0.25f;
        float dynamicSpread =
            maxSpreadAngle * (1f - Mathf.Exp(-k * (branchCount - 1)));

        float interval = (burstCount <= 1)
            ? 0f
            : minInterval + (maxInterval - minInterval) * Mathf.Exp(-decayK * (burstCount - 2));
        Debug.Log(2);


        // 🔸 n번 연속 발사
        for (int n = 0; n < burstCount; n++)
        {
            // 시전할 때마다 플레이어 시점 갱신
            forward = SkillManager.GetForwardDirection();
            spawnPos = SkillManager.GetCameraPosition() + forward * distanceOffset;
            baseRot = Quaternion.LookRotation(forward);
            Debug.Log(3);


            // 🔸 가지 발사(부채꼴)
            for (int i = 0; i < branchCount; i++)
            {
                Debug.Log(4);

                float t = (branchCount == 1) ? 0f : (float)i / (branchCount - 1);
                float angle = Mathf.Lerp(-dynamicSpread / 2f, dynamicSpread / 2f, t);
                Quaternion shotRot = Quaternion.AngleAxis(angle, Vector3.up) * baseRot;
                Vector3 shotDir = shotRot * Vector3.forward;
                Debug.Log(5);


                // ==========================================
                //  🎯 투사체 생성
                // ==========================================
                GameObject projectile = ObjectPooler.Instance.Spawn(
                    projectilePrefab,
                    spawnPos,
                    shotRot
                );

                ProjectileComponent pc = projectile.GetComponent<ProjectileComponent>();
                Debug.Log(6);


                pc.SetPrefabRef(projectilePrefab);

                // 🔥 SkillManager 싱글톤 기반: owner는 자동 => baseDamage만 넘기면 됨
                pc.Initialize(GetDamage());
                Debug.Log(7);


                pc.SetDestroyComponent(lifeTime, penetrable);
                pc.SetMotionType(projectileMotion);
                pc.SetPhysicalComponent(target, shotDir * projectileSpeed, motionSpeed);
            }

            // 🔸 burst 간 딜레이
            if (interval > 0f && n < burstCount - 1)
                yield return new WaitForSeconds(interval);
        }
    }

    // ============================================================
    // 패턴 조절 함수
    // ============================================================
    public void IncreaseBranchCount(int n = 1) => branchCount += n;
    public void DecreaseBranchCount(int n = 1) => branchCount = Mathf.Max(1, branchCount - n);

    public void IncreaseBurstCount(int n = 1) => burstCount += n;
    public void DecreaseBurstCount(int n = 1) => burstCount = Mathf.Max(1, burstCount - n);

    public int GetBranchCount() => branchCount;
    public int GetBurstCount() => burstCount;
}
