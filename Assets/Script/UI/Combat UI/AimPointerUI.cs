using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AimPointerUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Image aimImage;        // 조준점 이미지
    private RectTransform rect;

    [Header("크기 설정")]
    [SerializeField] private float baseScale = 1f;       // 평소 스케일
    [SerializeField] private float maxExtraScale = 0.4f; // 최대 추가 스케일 (1 + 0.4 = 1.4배 느낌)

    [Header("색 설정")]
    [SerializeField] private Color normalColor = Color.white;          // 평소 색
    [SerializeField] private Color hitColor = new Color(1f, 0.2f, 0.2f); // 강타시 붉은색

    [Header("데미지 → 강도 변환")]
    [SerializeField] private float damageScaleK = 100f; // 클수록 더 천천히 강해짐

    [Header("이펙트 시간 / 커브")]
    [SerializeField] private float effectDuration = 0.15f;
    [SerializeField]
    private AnimationCurve intensityCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // 처음 강하고 점점 줄어드는 형태

    private Coroutine effectCo;
    private float currentIntensity = 0f;   // 0~1, 이번 히트의 “세기”

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (aimImage == null)
            aimImage = GetComponent<Image>();

        // 초기 상태 저장
        rect.localScale = Vector3.one * baseScale;
        if (aimImage != null)
            aimImage.color = normalColor;
    }

    // 데미지를 넣을 때마다 호출. 반복 호출되면 강도만 갱신하여 재시작.
    public void OnDealDamage(float damage)
    {
        // 1) 데미지를 0~1 사이 강도로 압축 (포화 함수)
        //    t = damage / (damage + k)
        float t = damage / (damage + damageScaleK);
        float newIntensity = Mathf.Clamp01(t);

        // 2) 이미 이펙트가 돌고 있다면,
        //    기존 intensity와 비교해서 더 강한 쪽을 사용 (연속 히트 대비)
        currentIntensity = Mathf.Max(currentIntensity, newIntensity);

        // 3) 코루틴 다시 시작해서 효과 리셋 (짧은 타격감이 계속 이어지게)
        if (effectCo != null)
            StopCoroutine(effectCo);
        effectCo = StartCoroutine(HitEffectCoroutine());
    }

    private IEnumerator HitEffectCoroutine()
    {
        float time = 0f;

        while (time < effectDuration)
        {
            float normalized = time / effectDuration;     // 0 → 1
            float curve = intensityCurve.Evaluate(normalized); // 처음 1, 나중 0

            // 이번 프레임의 실제 강도
            float intensity = currentIntensity * curve;   // 0~1

            // 🔹 스케일 조절
            float scale = baseScale + maxExtraScale * intensity;
            rect.localScale = Vector3.one * scale;

            // 🔹 색 조절 (normal ↔ hitColor 사이 보간)
            if (aimImage != null)
            {
                Color c = Color.Lerp(normalColor, hitColor, intensity);
                aimImage.color = c;
            }

            time += Time.deltaTime;
            yield return null;
        }

        // 원래 상태로 복귀
        rect.localScale = Vector3.one * baseScale;
        if (aimImage != null)
            aimImage.color = normalColor;

        currentIntensity = 0f;
        effectCo = null;
    }
}
