using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WaveTimerUI : MonoBehaviour
{
    [SerializeField] RectTransform discGroup;   // 회전의 주체
    [SerializeField] Image dayDisc;
    [SerializeField] Image nightDisc;
    [SerializeField] TextMeshProUGUI inGameTime;

    void Update()
    {
        if (inGameTime != null)
            inGameTime.text = FormatTime((float)TimeManager.Instance.ElapsedTime);
    }

    public void UpdateRotation(float progress)
    {
        // 반시계 방향 회전
        float z = 360f * progress;
        discGroup.localRotation = Quaternion.Euler(0, 0, z);
    }

    public void SetRatio(float dayRatio)
    {
        dayRatio = Mathf.Clamp01(dayRatio);
        dayDisc.fillAmount = dayRatio;
        nightDisc.fillAmount = 1f - dayRatio;
    }

    private string FormatTime(float seconds)
    {
        int totalSeconds = Mathf.FloorToInt(seconds);
        int hour = totalSeconds / 3600;
        totalSeconds = totalSeconds % 3600;
        int min = totalSeconds / 60;
        int sec = totalSeconds % 60;
        return $"{hour:00}:{min:00}:{sec:00}";
    }
}
