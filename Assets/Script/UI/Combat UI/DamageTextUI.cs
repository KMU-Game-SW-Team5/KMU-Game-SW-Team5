using UnityEngine;
using TMPro;

public class DamageTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private float lifeTime = 0.7f;
    [SerializeField] private GameObject DamageTextObjectRef;

    [Header("스크린 좌표에서 상하좌우 랜덤 오프셋 범위 (픽셀)")]
    [SerializeField] private float screenJitterRadius = 40f;    // 좌우/상하 랜덤

    [Header("위로 떠오르는 픽셀 거리")]
    [SerializeField] private float floatUpPixels = 40f;

    [Header("알파 페이드 커브 (0~1)")]
    [SerializeField]
    private AnimationCurve alphaCurve =
        AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Header("데미지에 따른 폰트 크기")]
    [SerializeField] private float minFontSize = 24f;    // 최소 폰트
    [SerializeField] private float maxFontSize = 60f;    // 최대 폰트
    [SerializeField] private float fontSizeScaler = 100f;  // 스케일 조절용 변수

    [Header("데미지에 따른 색상")]
    [SerializeField] private Color lowDamageColor = Color.white;              // 약한 피해
    [SerializeField] private Color highDamageColor = new Color(1f, 0.2f, 0.2f); // 강한 피해(진한 빨강)
    [SerializeField] private float fontColorScaler = 100f;                 // 색 변화 스케일

    [Header("크리티컬 연출")]
    [SerializeField] private Color critColor = new Color(1f, 0.9f, 0.2f);   // 크리티컬 전용 강조 색
    [SerializeField] private float critFontScale = 1.3f;                    // 크리티컬 시 폰트 크기 배율 (1.3 = 30% 증가)

    private Transform target;          // 몬스터가 넘겨준 기준점(머리 위치용)
    private float timer = 0f;
    private Vector2 screenOffset;      // 각 텍스트마다 고정되는 랜덤 오프셋
    private Camera cam;

    public void Setup(float damage, Transform targetTransform, bool isCritical)
    {
        if (damage < 10f)
            Despawn();
        target = targetTransform;
        cam = Camera.main;

        if (text != null)
        {
            text.text = Mathf.RoundToInt(damage).ToString();

            // 🔸 1) 데미지를 0~1로 압축하는 포화 함수
            float t_font = damage / (damage + fontSizeScaler);
            float t_color = damage / (damage + fontColorScaler);

            // 🔸 2) 폰트 크기 보간
            float fontSize = Mathf.Lerp(minFontSize, maxFontSize, t_font);

            // 🔸 3) 기본 색상(데미지에 따른 색)
            Color baseColor = Color.Lerp(lowDamageColor, highDamageColor, t_color);

            // 🔸 4) 크리티컬이면 추가 연출 (폰트 더 크게 + 색상 변경)
            if (isCritical)
            {
                // 폰트 조금 더 키우기
                fontSize *= critFontScale;

                // 기존 색과 크리티컬 색을 섞어서 더 눈에 띄게
                baseColor = Color.Lerp(baseColor, critColor, 0.8f);
            }

            text.fontSize = fontSize;
            text.color = baseColor;   // RGB 세팅 (알파는 UpdateVisual에서 따로 처리)
        }

        // 스크린 랜덤 오프셋 등 기존 로직
        screenOffset = Random.insideUnitCircle * screenJitterRadius;
        if (damage < 100f)
        {
            screenOffset.y -= 30f;
            text.fontSize = minFontSize;
            text.color = lowDamageColor;
        }
        else
        {
            screenOffset.y = 0f; // 수평 오프셋만 적용
        }
        UpdateVisual(0f);   // 첫 프레임 렌더 전에 바로 위치/알파 세팅
    }

    private void Update()
    {
        timer += Time.deltaTime;
        float t = timer / lifeTime;

        if (t >= 1f)
        {
            Despawn();
            return;
        }

        UpdateVisual(t);
    }

    private void UpdateVisual(float t)
    {
        if (cam == null) cam = Camera.main;
        if (target == null) return;

        // 1) 기준이 되는 머리 위치(월드 → 스크린)
        Vector3 baseScreenPos = cam.WorldToScreenPoint(target.position);

        // 2) 스크린 상에서:
        //    - 고정 랜덤 오프셋 (screenOffset)
        //    - 위로 천천히 떠오르는 오프셋 (floatUpPixels * t)
        Vector3 screenPos =
            baseScreenPos +
            (Vector3)screenOffset +
            Vector3.up * (floatUpPixels * t);

        ((RectTransform)transform).position = screenPos;

        // 3) 알파 페이드
        if (text != null)
        {
            Color c = text.color;
            c.a = alphaCurve != null ? alphaCurve.Evaluate(t) : (1f - t);
            text.color = c;
        }
    }

    private void Despawn()
    {
        ObjectPooler.Instance.Despawn(DamageTextObjectRef, gameObject);
    }
}
