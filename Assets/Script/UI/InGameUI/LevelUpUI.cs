using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LevelUpUI : MonoBehaviour
{
    [SerializeField] private SkillCard[] cards;
    [Header("UI Animation")]
    [SerializeField] private float animDuration = 0.25f;
    [SerializeField] private Vector3 startScale = new Vector3(0.6f, 0.6f, 1f);
    [SerializeField] private float overshootScale = 1.1f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Coroutine animRoutine;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        InputBlocker.Block();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 애니메이션 시작
        PlayPopAnim();
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        InputBlocker.Unblock();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowCards()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            Debug.Log($"Show i :: {i}");
            cards[i].gameObject.SetActive(true);
        }
    }

    public void CloseSkillChoiceUI()
    {
        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].GetComponent<SkillCard>().Close();
        }
        gameObject.SetActive(false);
    }

    private void PlayPopAnim()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(PopAnim());
    }

    private IEnumerator PopAnim()
    {
        float t = 0f;
        rectTransform.localScale = startScale;
        canvasGroup.alpha = 0f;

        // 1단계: startScale → overshootScale
        float half = animDuration * 0.6f; // 60% 구간
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t / half);
            float eased = EaseOutQuad(n);

            rectTransform.localScale = Vector3.Lerp(startScale, Vector3.one * overshootScale, eased);
            canvasGroup.alpha = n;
            yield return null;
        }

        // 2단계: overshoot → 1.0으로 살짝 되돌아오기 (탄성 줄어드는 느낌)
        float t2 = 0f;
        float tail = animDuration - half;
        Vector3 from = Vector3.one * overshootScale;
        Vector3 to = Vector3.one;

        while (t2 < tail)
        {
            t2 += Time.unscaledDeltaTime;
            float n = Mathf.Clamp01(t2 / tail);
            float eased = EaseOutQuad(n);

            rectTransform.localScale = Vector3.Lerp(from, to, eased);
            yield return null;
        }

        rectTransform.localScale = Vector3.one;
        canvasGroup.alpha = 1f;
    }

    private float EaseOutQuad(float x)
    {
        return 1f - (1f - x) * (1f - x);
    }    
}
