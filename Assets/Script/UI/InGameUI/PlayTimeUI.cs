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

        // 초 단위로 변환
        long totalSeconds = (long)TimeManager.Instance.ElapsedTime;
        long hours = totalSeconds / 3600;
        long minutes = (totalSeconds % 3600) / 60;
        long seconds = totalSeconds % 60;

        // hh:mm:ss 형식 (hours는 2자리 이상으로 자동 확장)
        string formatted = $"{hours:00} : {minutes:00} : {seconds:00}";

        // 변경된 경우에만 텍스트 갱신
        if (formatted != lastText)
        {
            timeText.text = formatted;
            lastText = formatted;
        }
    }
}