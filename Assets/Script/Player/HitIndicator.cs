using System.Collections;
using UnityEngine;
using TMPro;

public class HitIndicator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hitText; // UI 텍스트 연결
    [SerializeField] private float displayDuration = 0.3f; // Hit 출력 시간

    private Coroutine displayCoroutine;


    private void OnEnable()
    {
        EventManager.OnMonsterHit += ShowHitIndicator;
    }

    // 메모리 누수 방지
    private void OnDisable()
    {
        EventManager.OnMonsterHit -= ShowHitIndicator;
    }

    private void ShowHitIndicator()
    {
        // HIT 표시가 실행 중이라면 중복 실행 X
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        displayCoroutine = StartCoroutine(HitIndicatorCoroutine());
    }

    private IEnumerator HitIndicatorCoroutine()
    {
        hitText.gameObject.SetActive(true);
     
        yield return new WaitForSeconds(displayDuration);
        // HIT 비활성화
        hitText.gameObject.SetActive(false);
    }
}