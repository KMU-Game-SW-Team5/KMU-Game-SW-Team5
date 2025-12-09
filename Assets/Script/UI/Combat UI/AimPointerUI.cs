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

    [Header("이펙트 시간 / 커브")]
    [SerializeField] private float effectDuration = 0.15f;
    [SerializeField]
    private AnimationCurve intensityCurve =
        AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // 처음 강하고 점점 줄어드는 형태

    [Header("데미지 -> 강도 매핑")]
    [Tooltip("이 값 이하의 데미지에는 거의 반응하지 않습니다")]
    [SerializeField] private float minDamageThreshold = 10f;
    [Tooltip("이 값을 기준으로 데미지에 비례해 강도가 커지며, 이를 넘으면 강도는 최대치(1)로 수렴합니다")]
    [SerializeField] private float maxDamageForScaling = 200f;

    [Header("수렴 / 소멸 속도")]
    [Tooltip("현재 강도가 목표 강도로 수렴하는 속도 (단위: intensity / 초)")]
    [SerializeField] private float intensityConvergenceSpeed = 3f;
    [Tooltip("이펙트가 끝난 뒤 강도가 0으로 사라지는 속도 (단위: intensity / 초)")]
    [SerializeField] private float intensityDecaySpeed = 2f;

    private Coroutine effectCo;
    private Coroutine convergeCo;
    private Coroutine decayCo;

    private float currentIntensity = 0f;   // 0~1, 렌더링 시 사용되는 '실제' 강도 (수렴/감쇠에 의해 갱신)
    private float targetIntensity = 0f;    // OnDealDamage가 계산한 목표 강도 (0~1)

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

    // 데미지를 넣을 때마다 호출.
    // 이제 작은 데미지(< minDamageThreshold)에는 반응하지 않고,
    // 그 이상에서는 min..max 범위에 따라 0..1로 정규화된 목표 강도로 수렴합니다.
    public void OnDealDamage(float damage)
    {
        // 음수 방지
        damage = Mathf.Max(0f, damage);

        // 임계치 이하이면 무시(거의 반응하지 않음)
        if (damage <= minDamageThreshold)
            return;

        // damage를 [minDamageThreshold, maxDamageForScaling] -> [0,1] 범위로 정규화
        float denom = Mathf.Max(0.0001f, maxDamageForScaling - minDamageThreshold);
        float normalized = Mathf.Clamp01((damage - minDamageThreshold) / denom);

        // 목표 강도 설정
        targetIntensity = normalized;

        // 기존 감소 코루틴이 돌고 있으면 멈춰서 새로운 목표에 수렴하게 함
        if (decayCo != null)
        {
            StopCoroutine(decayCo);
            decayCo = null;
        }

        // 수렴 코루틴 시작(이미 돌고 있으면 재시작하지 않고 계속 수렴)
        if (convergeCo == null)
            convergeCo = StartCoroutine(ConvergeIntensityRoutine());
        // 만약 이미 수렴 중이라도 목표가 더 클 경우 즉시 반영될 수 있게 허용(ConvergeRoutine 내부에서 처리)

        // 이펙트 코루틴은 매 타격마다 재시작(짧은 타격감이 계속 이어지도록)
        if (effectCo != null)
            StopCoroutine(effectCo);
        effectCo = StartCoroutine(HitEffectCoroutine());
    }

    // currentIntensity를 targetIntensity로 부드럽게 이동시킴
    private IEnumerator ConvergeIntensityRoutine()
    {
        // 빠른 응답을 위해 루프에서 MoveTowards 사용
        while (!Mathf.Approximately(currentIntensity, targetIntensity))
        {
            currentIntensity = Mathf.MoveTowards(currentIntensity, targetIntensity, intensityConvergenceSpeed * Time.deltaTime);
            yield return null;
        }

        // 도달했으면 코루틴 종료 핸들링
        convergeCo = null;
    }

    // 이펙트 코루틴: effectDuration 동안 intensityCurve로 시각적 강도 적용
    private IEnumerator HitEffectCoroutine()
    {
        float time = 0f;

        while (time < effectDuration)
        {
            float normalizedTime = time / effectDuration;     // 0 → 1
            float curve = intensityCurve.Evaluate(normalizedTime); // 1→0 형태

            // 실제 렌더링 강도: 현재 수렴된 강도(currentIntensity)에 커브를 곱함
            float intensity = currentIntensity * curve;   // 0~1

            // 스케일 조절
            float scale = baseScale + maxExtraScale * intensity;
            rect.localScale = Vector3.one * scale;

            // 색 조절 (normal ↔ hitColor 사이 보간)
            if (aimImage != null)
            {
                Color c = Color.Lerp(normalColor, hitColor, intensity);
                aimImage.color = c;
            }

            time += Time.deltaTime;
            yield return null;
        }

        // 이펙트 종료 시 시각적 원복
        rect.localScale = Vector3.one * baseScale;
        if (aimImage != null)
            aimImage.color = normalColor;

        effectCo = null;

        // 이펙트가 끝난 뒤 수렴 코루틴이 없다면 강도를 천천히 0으로 만들자
        if (convergeCo == null && currentIntensity > 0f)
        {
            if (decayCo != null) StopCoroutine(decayCo);
            decayCo = StartCoroutine(DecayIntensityRoutine());
        }
    }

    // currentIntensity를 0으로 천천히 줄임
    private IEnumerator DecayIntensityRoutine()
    {
        while (currentIntensity > 0.0001f)
        {
            currentIntensity = Mathf.MoveTowards(currentIntensity, 0f, intensityDecaySpeed * Time.deltaTime);
            yield return null;
        }

        currentIntensity = 0f;
        decayCo = null;
    }
}
