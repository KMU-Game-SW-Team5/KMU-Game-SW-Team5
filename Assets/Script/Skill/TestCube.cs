using UnityEngine;

public class TestCube : MonoBehaviour
{
    [Header("움직임 설정")]
    [SerializeField] float moveSpeed = 2f;            // 단위/초
    [SerializeField] float moveDistance = 3f;         // 중심에서 한쪽 끝까지
    [SerializeField] Vector3 moveDirection = Vector3.right;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        moveDirection = moveDirection.normalized;
    }

    void Update()
    {
        float offset = Mathf.PingPong(Time.time * moveSpeed, 2f * moveDistance) - moveDistance;
        transform.position = startPos + moveDirection * offset;
    }
}
