using UnityEngine;

public class WaveTimerUI : MonoBehaviour
{
    [SerializeField] RectTransform timerDisc; // 가운데 원판
    [SerializeField] TimeManager timeManager;

    public void UpdateRotation(float progress)
    {
        // 반시계 방향 회전
        float z = -90f + (360f * progress);
        timerDisc.localRotation = Quaternion.Euler(0, 0, z);
    }
}
