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
            
            // �ʱ� ����ȭ
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
        Debug.Log($"KillCountUI: Kill count updated to {totalKills}");
        textKillCount.text = $"óġ ��: {totalKills}";
    }
}
