using UnityEngine;

/// 스킬 시전 위치를 나타내는 앵커.
/// RaycastHit 지점에 생성되며, 필요시 충돌 대상에 부착 가능.
public class SkillAnchor : MonoBehaviour
{
    [Header("기본 설정")]
    [Tooltip("일정 시간이 지나면 자동으로 파괴")]
    [SerializeField] private float lifetime = 10f;

    [Header("부착 관련 설정")]
    [Tooltip("충돌한 오브젝트에 자동 부착할지 여부")]
    [SerializeField] private bool attachToTarget = false;

    [Tooltip("부착 시의 오프셋")]
    [SerializeField] private Vector3 attachOffset = Vector3.zero;

    private bool isAttached = false;
    private Transform attachedTarget;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("SkillAnchor");
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!attachToTarget || isAttached) return;

        // 대상에 부착
        transform.SetParent(other.transform);
        transform.localPosition = attachOffset;
        isAttached = true;
    }

    // 수동으로 부착 처리 (RaycastHit 결과를 이용)
    public void AttachTo(Transform target, Vector3 hitPoint)
    {
        if (!attachToTarget || target == null || isAttached)
            return;

        attachedTarget = target;
        transform.SetParent(target, worldPositionStays: true);
        transform.position = hitPoint + attachOffset;
        isAttached = true;
    }
}
