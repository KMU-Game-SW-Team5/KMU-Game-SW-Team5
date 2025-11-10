using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Motion/Parabolic")]
public class ParabolicMotion : Motion
{
    // 멤버인 현재 속도 벡터를 계속 업데이트 해주는 역할을 함.
    override public Vector3 GetNextVelocity(Transform target, Vector3 velocity, Vector3 acceleration)
    {
        Debug.Log("이전 속도 : " + velocity.ToString());
        Debug.Log("나중 속도 : " + (velocity + acceleration * Time.fixedDeltaTime).ToString());
        return velocity + acceleration * Time.fixedDeltaTime;
    }

}
