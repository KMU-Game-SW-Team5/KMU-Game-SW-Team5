using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspect : MonoBehaviour
{
    [SerializeField] private float targetWidth = 16f;
    [SerializeField] private float targetHeight = 9f;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        ApplyLetterbox(); // 처음 한 번 적용
    }

    private void Update()
    {
        // 해상도 / Game 뷰 크기가 바뀌었는지 체크
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ApplyLetterbox();
        }
    }

    private void ApplyLetterbox()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float targetAspect = targetWidth / targetHeight;
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = cam.rect;

        if (scaleHeight < 1f)
        {
            // 화면이 더 세로로 길다 → 위/아래에 여백
            rect.width = 1f;
            rect.height = scaleHeight;
            rect.x = 0f;
            rect.y = (1f - scaleHeight) / 2f;
        }
        else
        {
            // 화면이 더 가로로 넓다 → 좌/우에 여백
            float scaleWidth = 1f / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1f;
            rect.x = (1f - scaleWidth) / 2f;
            rect.y = 0f;
        }

        cam.rect = rect;
    }
}
