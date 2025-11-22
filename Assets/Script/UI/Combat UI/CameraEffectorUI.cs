using UnityEngine;
using UnityEngine.UI;

public class CameraEffectorUI : MonoBehaviour
{
    [Header("텍스처 설정")]
    [SerializeField] private int textureSize = 1024;

    [Header("색 설정")]
    [SerializeField] private Color lowHpColor = new Color(1f, 0f, 0f, 1f);

    [Header("반경 및 부드러움")]
    [SerializeField, Tooltip("0~1, 이 값까지는 알파=0 (화면 중앙)")]
    private float innerRadius = 0.4f;
    [SerializeField, Tooltip("0~1, 이 값 이후로 알파=1 (최외곽)")]
    private float outerRadius = 0.9f;

    [Header("Low HP 깜빡임 설정")]
    [SerializeField] private float blinkMaxIntensity = 0.8f;
    [SerializeField] private float blinkSpeed = 2.0f;

    [Header("Low HP 비네트 오브젝트")]
    [SerializeField] private Image lowHpImage;          // 비네트 이미지
    [SerializeField] private CanvasGroup lowHpCanvas;   // 비네트용 CG

    [Header("Hit Flash 오브젝트")]
    [SerializeField] private CanvasGroup hitFlashCanvas; // 피격 플래시용 CG
    [SerializeField] private float hitFlashMaxAlpha = 0.6f;
    [SerializeField] private float hitFlashDuration = 0.15f;

    private Coroutine blinkRoutine;
    private Coroutine hitFlashRoutine;

    private void Awake()
    {
        if (lowHpCanvas != null)
            lowHpCanvas.alpha = 0f;

        if (hitFlashCanvas != null)
            hitFlashCanvas.alpha = 0f;

        if (lowHpImage != null)
            GenerateVignette(lowHpImage);
    }

    private void GenerateVignette(Image targetImage)
    {
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        float half = textureSize / 2f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float nx = (x - half) / half;
                float ny = (y - half) / half;
                float dist = Mathf.Sqrt(nx * nx + ny * ny);

                float alpha;
                if (dist <= innerRadius)
                    alpha = 0f;
                else if (dist >= outerRadius)
                    alpha = 1f;
                else
                {
                    float t = Mathf.InverseLerp(innerRadius, outerRadius, dist);
                    alpha = t * t * (3f - 2f * t);
                }

                Color c = lowHpColor;
                c.a = alpha;
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();

        Sprite sp = Sprite.Create(
            tex,
            new Rect(0, 0, textureSize, textureSize),
            new Vector2(0.5f, 0.5f)
        );

        targetImage.sprite = sp;
        targetImage.type = Image.Type.Simple;
        targetImage.preserveAspect = true;
        targetImage.color = Color.white;
    }

    // -------- Low HP 깜빡임 --------
    public void StartLowHpBlink()
    {
        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = StartCoroutine(BlinkCoroutine());
    }

    public void StopLowHpBlink()
    {
        if (blinkRoutine != null)
        {
            StopCoroutine(blinkRoutine);
            blinkRoutine = null;
        }

        if (lowHpCanvas != null)
            lowHpCanvas.alpha = 0f;
    }

    private System.Collections.IEnumerator BlinkCoroutine()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * blinkSpeed;

            float s = (Mathf.Sin(t) + 1f) * 0.5f; // 0~1
            float intensity = Mathf.Lerp(0f, blinkMaxIntensity, s);

            if (lowHpCanvas != null)
                lowHpCanvas.alpha = intensity;

            yield return null;
        }
    }

    // -------- Hit Flash --------
    public void PlayHitFlash(float intensity01 = 1f)
    {
        if (hitFlashCanvas == null) return;

        intensity01 = Mathf.Clamp01(intensity01);
        float targetAlpha = hitFlashMaxAlpha * intensity01;

        if (hitFlashRoutine != null)
            StopCoroutine(hitFlashRoutine);

        hitFlashRoutine = StartCoroutine(HitFlashCoroutine(targetAlpha));
    }

    private System.Collections.IEnumerator HitFlashCoroutine(float targetAlpha)
    {
        hitFlashCanvas.alpha = targetAlpha;

        float t = 0f;
        while (t < hitFlashDuration)
        {
            t += Time.deltaTime;
            float k = t / hitFlashDuration;
            hitFlashCanvas.alpha = Mathf.Lerp(targetAlpha, 0f, k);
            yield return null;
        }

        hitFlashCanvas.alpha = 0f;
        hitFlashRoutine = null;
    }
}

