using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class MatchViewportUI : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (targetCamera == null)
            targetCamera = Camera.main;

        Apply();
    }

    private void LateUpdate()
    {
        // 해상도나 카메라 rect가 바뀔 수 있으니 매 프레임(or 조건) 재적용
        Apply();
    }

    private void Apply()
    {
        if (targetCamera == null) return;

        Rect r = targetCamera.rect;  // normalized (0~1)

        // 카메라 rect와 동일하게 앵커 설정
        rectTransform.anchorMin = new Vector2(r.x, r.y);
        rectTransform.anchorMax = new Vector2(r.x + r.width, r.y + r.height);

        // 앵커 기준으로 정확히 딱 맞게
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
