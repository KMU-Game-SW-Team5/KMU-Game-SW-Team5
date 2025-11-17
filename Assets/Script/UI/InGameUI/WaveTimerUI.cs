using UnityEngine;
using UnityEngine.UI;

public class WaveTimerUI : MonoBehaviour
{
    [SerializeField] RectTransform discGroup;   // 회전의 주체
    [SerializeField] Image dayDisc;
    [SerializeField] Image nightDisc;

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
}
