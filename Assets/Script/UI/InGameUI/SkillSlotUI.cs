using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SkillSlotUI : MonoBehaviour
{
    [SerializeField] private Image cooldownFill;
    [SerializeField] private float cooldownTime;
    private SpriteRenderer iconSR;
    private Image iconImage;
    private bool isCoolingDown = false;
    private float elapsed = 0f;

    [Header("Cooldown Flash")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.25f; // 전체 플래시 시간
    [SerializeField] private float flashScale = 1.15f;    // 살짝 커지는 스케일
    private Coroutine flashCoroutine;

    private Color originalFillColor;
    private Vector3 originalFillScale;

    private float lastRatio = 0f; // SetCooldownRatio로만 쿨을 관리할 때 경계 검사용

    private void Awake()
    {
        // 아이콘 이미지 찾기 (cooldownFill과는 다른 Image를 찾아서 아이콘으로 사용)
        iconSR = GetComponentInChildren<SpriteRenderer>();

        if (cooldownFill != null)
        {
            originalFillColor = cooldownFill.color;
            originalFillScale = cooldownFill.rectTransform.localScale;
        }

        if (iconImage == null)
        {
            var images = GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img != cooldownFill)
                {
                    iconImage = img;
                    break;
                }
            }
        }
    }

    // 남은 쿨타임 비율을 입력받아서 UI를 세팅함.
    // 0 -> >0 : 시전 플래시
    // >0 -> 0 : 쿨 종료 플래시
    public void SetCooldownRatio(float ratio)
    {
        if (cooldownFill == null) return;

        float clamped = Mathf.Clamp01(ratio);
        cooldownFill.fillAmount = clamped;

        // 0 -> >0 : 쿨타임 시작 (시전)
        if (Mathf.Approximately(lastRatio, 0f) && clamped > 0f)
        {
            StartFlash();
            isCoolingDown = true;
        }
        // >0 -> 0 : 쿨타임 종료
        else if (lastRatio > 0f && Mathf.Approximately(clamped, 0f))
        {
            isCoolingDown = false;
            StartFlash();
        }

        lastRatio = clamped;
    }

    public void ActivateCooldown(float time)
    {
        if (isCoolingDown) return;

        cooldownTime = time;

        // 혹시 이전 플래시가 돌고 있으면 정리
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
            RestoreFillVisual();
        }

        // 시전 순간 플래시
        StartFlash();

        isCoolingDown = true;
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        if (cooldownFill == null)
        {
            isCoolingDown = false;
            yield break;
        }

        elapsed = 0f;
        cooldownFill.fillAmount = 1f;
        lastRatio = 1f;

        while (elapsed < cooldownTime)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / cooldownTime);
            float remaining = 1f - ratio;
            cooldownFill.fillAmount = remaining;
            lastRatio = remaining;

            yield return null;
        }

        cooldownFill.fillAmount = 0f;
        lastRatio = 0f;
        isCoolingDown = false;

        // 쿨타임 종료 플래시
        StartFlash();
    }

    private void StartFlash()
    {
        if (cooldownFill == null) return;

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }

        flashCoroutine = StartCoroutine(FlashCooldownFill());
    }

    private IEnumerator FlashCooldownFill()
    {
        if (cooldownFill == null)
        {
            flashCoroutine = null;
            yield break;
        }

        float half = Mathf.Max(0.0001f, flashDuration * 0.5f);

        // 1단계: 원래 -> 플래시
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float p = t / half;
            cooldownFill.color = Color.Lerp(originalFillColor, flashColor, p);
            cooldownFill.rectTransform.localScale =
                Vector3.Lerp(originalFillScale, originalFillScale * flashScale, p);
            yield return null;
        }

        // 피크 고정
        cooldownFill.color = flashColor;
        cooldownFill.rectTransform.localScale = originalFillScale * flashScale;

        // 2단계: 플래시 -> 원래
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            float p = t / half;
            cooldownFill.color = Color.Lerp(flashColor, originalFillColor, p);
            cooldownFill.rectTransform.localScale =
                Vector3.Lerp(originalFillScale * flashScale, originalFillScale, p);
            yield return null;
        }

        RestoreFillVisual();
        flashCoroutine = null;
    }

    private void RestoreFillVisual()
    {
        if (cooldownFill == null) return;

        cooldownFill.color = originalFillColor;
        cooldownFill.rectTransform.localScale = originalFillScale;
    }

    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
            iconImage.color = Color.white;
        }
        else if (iconSR != null)
        {
            iconSR.sprite = sprite;
            iconSR.color = Color.white;
        }
    }
}
