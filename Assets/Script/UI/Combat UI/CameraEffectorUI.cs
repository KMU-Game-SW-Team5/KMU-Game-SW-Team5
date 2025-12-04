using System.Collections;
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

    [Header("사각형 테두리 설정")]
    [SerializeField, Range(0f, 0.5f)]
    private float borderThickness = 0.1f;   // 0에 가까울수록 얇은 테두리


    private float lastAspect = -1f;   // 마지막으로 생성했을 때의 화면 비율


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


    [SerializeField] private Transform camTransform;
    [SerializeField] private float defaultShakeIntensity = 0.5f;
    [SerializeField] private float defaultShakeDuration = 0.2f;

    private Coroutine shakeCo;
    private Vector3 originalLocalPos;

    private void Awake()
    {
        if (lowHpCanvas != null)
            lowHpCanvas.alpha = 0f;

        if (hitFlashCanvas != null)
            hitFlashCanvas.alpha = 0f;

        if (lowHpImage != null)
        {
            GenerateVignette(lowHpImage);
            lastAspect = GetCurrentAspect();
        }

        if (skillScreenEffectCanvas != null)
            skillScreenEffectCanvas.alpha = 0f;
    }

    private void Start()
    {
        camTransform = Camera.main.transform;
    }

    private void Update()
    {
        // 카메라 셋업 등 기존 Update 내용이 있다면 그 위/아래에 추가

        float currentAspect = GetCurrentAspect();

        // 비율이 일정 이상 바뀌었을 때만 다시 생성 (0.01 = 약 1% 차이)
        if (Mathf.Abs(currentAspect - lastAspect) > 0.01f)
        {
            if (lowHpImage != null)
            {
                GenerateVignette(lowHpImage);
                lastAspect = currentAspect;
            }
        }
    }


    private void GenerateVignette(Image targetImage)
    {
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;

        int w = textureSize;
        int h = textureSize;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // 0 ~ 1 정규화 좌표
                float u = x / (w - 1f);
                float v = y / (h - 1f);

                // 네 개의 가장자리까지의 거리 중 최소값
                // (0쯤이면 화면 가장자리, 0.5로 갈수록 중앙)
                float distToEdge = Mathf.Min(u, v, 1f - u, 1f - v);

                // 테두리 두께 기준으로 알파 계산
                // distToEdge == 0        → 알파 최대
                // distToEdge == thickness→ 알파 0
                float alpha;
                if (distToEdge >= borderThickness)
                {
                    alpha = 0f; // 테두리 안쪽은 투명
                }
                else
                {
                    float t = distToEdge / Mathf.Max(borderThickness, 0.0001f);
                    alpha = 1f - t;      // 가장자리 1 → 안쪽으로 갈수록 0
                                         // 부드럽게 하고 싶으면 조금 둥글게
                    alpha = alpha * alpha * (3f - 2f * alpha);
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
        targetImage.preserveAspect = false;  // 화면 비율에 맞게 늘이기
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

    [SerializeField] Image skillScreenEffectImage;
    [SerializeField] CanvasGroup skillScreenEffectCanvas;
    private Coroutine skillScreenEffectRoutine;

    public void StartScreenColorEffect(float duration, Color color, float blinkPeriod)
    {
        Debug.Log("CameraEffectorUI: 화면 색상 효과 시작");
        if (skillScreenEffectImage == null || skillScreenEffectCanvas == null)
            return;

        // 이전 코루틴 중복 방지
        if (skillScreenEffectRoutine != null)
            StopCoroutine(skillScreenEffectRoutine);

        skillScreenEffectRoutine = StartCoroutine(SkillScreenEffectCoroutine(duration, color, blinkPeriod));
    }

    private IEnumerator SkillScreenEffectCoroutine(float duration, Color color, float blinkPeriod)
    {
        Debug.Log("CameraEffectorUI: 화면 색상 효과 코루틴 실행"); 
        if (skillScreenEffectImage == null || skillScreenEffectCanvas == null)
            yield break;

        // 안전값
        if (duration <= 0f) yield break;
        blinkPeriod = Mathf.Max(0.0001f, blinkPeriod);

        // 이미지는 색상의 RGB를 유지하되 알파는 1으로 두고,
        // CanvasGroup.alpha를 애니메이트해서 최종 alpha를 color.a로 만들자.
        skillScreenEffectImage.color = new Color(color.r, color.g, color.b, 1f);
        float targetAlpha = Mathf.Clamp01(color.a);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // 현재 사이클 위치: 0..1
            float cyclePos = (elapsed % blinkPeriod) / blinkPeriod;

            // 0..0.5 : 증가, 0.5..1 : 감소
            float phase = cyclePos * 2f;
            float alphaFactor;
            if (phase <= 1f)
            {
                alphaFactor = Mathf.SmoothStep(0f, 1f, phase); // 부드러운 증가
            }
            else
            {
                alphaFactor = Mathf.SmoothStep(1f, 0f, phase - 1f); // 부드러운 감소
            }

            skillScreenEffectCanvas.alpha = targetAlpha * alphaFactor;

            yield return null;
        }

        skillScreenEffectCanvas.alpha = 0f;
        skillScreenEffectRoutine = null;
    }



    public void StartCameraShake(float intensity, float duration)
    {
        if (shakeCo != null)
            StopCoroutine(shakeCo);
        shakeCo = StartCoroutine(ShakeRoutine(intensity, duration));
    }

    private System.Collections.IEnumerator ShakeRoutine(float intensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 점점 줄어드는 진폭
            float current = intensity * (1f - t);

            Vector3 offset = Random.insideUnitSphere * current;
            offset.z = 0f; // 필요하면 Z는 고정

            camTransform.localPosition = originalLocalPos + offset;

            yield return null;
        }

        camTransform.localPosition = originalLocalPos;
        shakeCo = null;
    }

    private float GetCurrentAspect()
    {
        if (Screen.height <= 0) return 0f;
        return (float)Screen.width / Screen.height;
    }

}

