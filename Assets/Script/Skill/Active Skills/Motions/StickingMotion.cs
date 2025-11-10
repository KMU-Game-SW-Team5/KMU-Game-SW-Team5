using UnityEngine;

// 조준한 대상에 부착시키거나 거리가 멀면 SkillManager에서 지정한 최대 거리에 생성
[CreateAssetMenu(menuName = "Scriptable Object/Motion/Sticking")]
public class StickingMotion : Motion
{
    // 생성한 스킬 자신의 rb를 지정한 위치에 고정
    override public Vector3 GetNextVelocity(Transform target, Vector3 velocity, Vector3 acceleration)
    {
        return velocity;
    }
}
