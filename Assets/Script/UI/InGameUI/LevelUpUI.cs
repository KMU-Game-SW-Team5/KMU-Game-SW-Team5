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
        // 뽑기 세션 시작: 이번 레벨업에서 중복 방지용 초기화
        SkillCard.BeginRollSession();

        // 카드 3장(또는 N장) 생성 + 활성화
        ShowCards();

        // 게임 멈추고 입력 / 마우스 설정
        Time.timeScale = 0f;
        InputBlocker.Block();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 팝업 애니메이션 시작
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
        if (cards == null) return;

        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;
            cards[i].gameObject.SetActive(true);   // 여기서 SkillCard.OnEnable → 뽑기 발생
        }
    }

    public void CloseSkillChoiceUI()
    {
        if (cards != null)
        {
            for (int i = 0; i < cards.Length; i++)
            {
                if (cards[i] == null) continue;
                cards[i].Close();   // 카드 비활성
            }
        }

        gameObject.SetActive(false); // UI 꺼짐 → 다음에 켜질 때 다시 BeginRollSession + ShowCards
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

        // 2단계: overshoot → 1.0으로 살짝 되돌아오기
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
