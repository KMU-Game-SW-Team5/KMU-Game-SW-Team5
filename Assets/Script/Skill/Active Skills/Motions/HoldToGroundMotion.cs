using UnityEngine;

// 지정한 지점 또는 방향의 정사영된 지점으로 배치
[CreateAssetMenu(menuName = "Scriptable Object/Motion/HoldToGround")]
public class HoldToGroundMotion : Motion
{
    [SerializeField] private LayerMask groundMask = Physics.DefaultRaycastLayers;
    [SerializeField] private float maxGroundCheckDistance = 100f;

    override public Vector3 GetNextVelocity(Transform target, Vector3 velocity, Vector3 acceleration)
    {
        if (target == null) return velocity;

        Vector3 startPos = target.position;
        Vector3 groundPos = startPos;

        // 앵커 아래로 Raycast를 쏴서 지면 확인
        if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, maxGroundCheckDistance, groundMask))
        {
            groundPos = hit.point;  // 맞은 지점이 실제 땅의 위치
        }

        return velocity;
    }
}
