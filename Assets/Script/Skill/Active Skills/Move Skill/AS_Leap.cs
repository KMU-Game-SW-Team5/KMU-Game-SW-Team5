using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "AS_Leap", menuName = "Scriptable Object/Active Skill/MoveType/Leap")]
public class AS_Leap : ActiveSkillBase
{
    [Header("도약 설정")]

    [SerializeField, Range(0f, 90f)]
    private float jumpAngle = 10f;           // 위로 뛰는 각도(도 단위)

    protected override void Execute(GameObject user, Transform target)
    {
        if (user == null) return;

        var mover = user.GetComponent<MoveController>();
        if (mover == null) return;

        // 1. 기본 방향: 카메라 또는 플레이어 전방
        Vector3 baseDir;
        if (SkillManager.cam != null)
            baseDir = SkillManager.cam.transform.forward;
        else
            baseDir = user.transform.forward;

        // 2. 수평 방향만 사용 (y = 0)
        Vector3 horizontalDir = new Vector3(baseDir.x, 0f, baseDir.z);
        if (horizontalDir.sqrMagnitude < 0.0001f)
            horizontalDir = user.transform.forward; // 혹시라도 0벡터 나오면 fallback

        horizontalDir.Normalize();

        // 3. jumpAngle만큼 위로 들어올린 방향 만들기
        float rad = jumpAngle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        // 수평과 위쪽을 비율대로 섞어서 각도 만든다
        Vector3 angledDir = horizontalDir * cos + Vector3.up * sin;
        angledDir.Normalize();

        // 4. Leap 호출 (dir은 normalized, 힘/거리 쪽은 baseValue)
        float power = baseValue; 


        mover.Leap(angledDir, power);
    }
}
