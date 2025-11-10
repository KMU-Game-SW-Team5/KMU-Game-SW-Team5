using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Motion/Parabolic")]
public class ParabolicMotion : Motion
{
    // 멤버인 현재 속도 벡터를 계속 업데이트 해주는 역할을 함.
    override public Vector3 GetNextVelocity(Transform target, Vector3 velocity, Vector3 acceleration)
    {
        return velocity + acceleration * Time.fixedDeltaTime;
    }

}
