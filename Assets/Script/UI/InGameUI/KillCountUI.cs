using TMPro;
using UnityEngine;

public class KillCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textKillCount;

    private void OnEnable()
    {
        if (KillCounter.Instance != null)
        {
            KillCounter.Instance.OnKillCountChanged += HandleKillChanged;
            
            // 초기 동기화
            HandleKillChanged(KillCounter.Instance.TotalKills);
        }
    }

    private void OnDisable()
    {
        if (KillCounter.Instance != null)
            KillCounter.Instance.OnKillCountChanged -= HandleKillChanged;
    }

    private void HandleKillChanged(int totalKills)
    {
        textKillCount.text = $"처치 수: {totalKills}";
    }
}
