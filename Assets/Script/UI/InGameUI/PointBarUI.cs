using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PointBarUI : MonoBehaviour
{
    [SerializeField] private Image pointBarFill;
    private float maxPoint;
    private float currentPoint;

    [Header("Fill Transition")]
    [SerializeField] private float fillTransitionDuration = 0.5f;
    [SerializeField] private AnimationCurve fillEasing = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // 텍스트
    [Header("text")]
    [SerializeField] private TextMeshProUGUI currentValueText;
    [SerializeField] private TextMeshProUGUI maxValueText;

    [Header("Options")]
    [Tooltip("true면 currentValueText를 백분율로 표시합니다 (소수점 1자리).")]
    [SerializeField] private bool usePercentage = false;

    private Coroutine fillCoroutine;

    public void SetPointUI(float newPoint, float newMax)
    {
        maxPoint = Mathf.Max(0.0001f, newMax);
        currentPoint = Mathf.Clamp(newPoint, 0f, maxPoint);

        UpdateFillBar();
    }

    private void UpdateFillBar()
    {
        if (pointBarFill == null) return;

        float target = Mathf.Clamp01(currentPoint / maxPoint);

        // 텍스트가 있으면 업데이트
        if (currentValueText != null)
        {
            if (usePercentage)
            {
                currentValueText.text = $"{target * 100 :F1}%";
            }
            else
            {
                currentValueText.text = Mathf.RoundToInt(currentPoint).ToString();
                if (maxValueText != null)
                {
                    maxValueText.text = Mathf.RoundToInt(maxPoint).ToString();
                }
            }
        }

        // 이전 코루틴 취소 후 새로 시작
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }

        if (fillTransitionDuration <= 0f)
        {
            pointBarFill.fillAmount = target;
        }
        else
        {
            fillCoroutine = StartCoroutine(AnimateFillCoroutine(pointBarFill.fillAmount, target, fillTransitionDuration));
        }
    }

    private IEnumerator AnimateFillCoroutine(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eval = fillEasing != null ? fillEasing.Evaluate(t) : t;
            pointBarFill.fillAmount = Mathf.LerpUnclamped(from, to, eval);
            yield return null;
        }

        pointBarFill.fillAmount = to;
        fillCoroutine = null;
    }
}