using TMPro;
using UnityEngine;

public class KillCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textKillCount;

    private void OnEnable()
    {
        if (KillCounter.Instance != null)
        {
            Register(KillCounter.Instance);
        }
        else
        {
            KillCounter.OnCreated += Register;
        }
    }

    private void OnDisable()
    {
        if (KillCounter.Instance != null)
            KillCounter.Instance.OnKillCountChanged -= HandleKillChanged;

        KillCounter.OnCreated -= Register;
    }

    private void HandleKillChanged(int totalKills)
    {
        Debug.Log($"KillCountUI: Kill count updated to {totalKills}");
        textKillCount.text = $"Kills: {totalKills}";
    }

    private void Register(KillCounter counter)
    {
        counter.OnKillCountChanged += HandleKillChanged;

        // 초기 동기화
        HandleKillChanged(counter.TotalKills);
    }
}
