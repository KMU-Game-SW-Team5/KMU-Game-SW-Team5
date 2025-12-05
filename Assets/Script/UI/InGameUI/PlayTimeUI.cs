using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayTimeUI : MonoBehaviour
{
    private TextMeshProUGUI timeText;
    private string lastText = string.Empty;

    private void Start()
    {
        timeText = GetComponent<TextMeshProUGUI>();
        if (timeText == null)
        {
            Debug.LogWarning($"{name}: TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");
        }
    }

    private void Update()
    {
        if (timeText == null) return;
        if (TimeManager.Instance == null) return;

        // 게임이 엔딩 대기/진행 상태이면 시간 텍스트 갱신 중지
        if (GameManager.Instance != null && GameManager.Instance.IsEnding) return;

        double elapsed = TimeManager.Instance.ElapsedTime; // 초 단위(소수 포함)

        // 소수점 둘째자리까지(centiseconds) 반올림하여 계산
        long totalCentiseconds = (long)System.Math.Round(elapsed * 100.0); // 전체 센티초(1/100초)
        long totalSeconds = totalCentiseconds / 100;
        int centiseconds = (int)(totalCentiseconds % 100);

        long minutes = (totalSeconds % 3600) / 60;
        long seconds = totalSeconds % 60;

        string formatted;
        // hh : mm : ss.cc (소수점 둘째자리)
        formatted = $"{minutes:00} : {seconds:00} : {centiseconds:00}";

        // 변경된 경우에만 텍스트 갱신
        if (formatted != lastText)
        {
            timeText.text = formatted;
            lastText = formatted;
        }
    }
}