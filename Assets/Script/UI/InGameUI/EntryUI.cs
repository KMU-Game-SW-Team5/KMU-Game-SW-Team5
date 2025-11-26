using TMPro;
using UnityEngine;

public class EntryUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI playTimeText;
    [SerializeField] private GameObject highlight;

    private void OnDisable()
    {
        highlight.SetActive(false);
    }

    public void SetData(LeaderboardEntry entry, int rank, bool isHighlight)
    {
        rankText.text = rank.ToString();
        nicknameText.text = entry.Nickname;
        switch (entry.Difficulty)
        {
            case 0:
                difficultyText.text = "Easy";
                break;
            case 1:
                difficultyText.text = "Normal";
                break;
            case 2:
                difficultyText.text = "Hard";
                break;

        }
        levelText.text = entry.LevelAchieved.ToString();
        killsText.text = entry.MonsterKills.ToString();
        playTimeText.text = FormatTime(entry.PlayTime);

        if (isHighlight)
        {
            highlight.SetActive(true);
        }
        Debug.Log($"[EntryUI] entry = {entry.Id}");
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
