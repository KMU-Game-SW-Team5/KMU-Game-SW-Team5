using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI playTime;
    [SerializeField] TextMeshProUGUI levelAchieved;
    [SerializeField] TextMeshProUGUI monsterKills;

    private void OnEnable()
    {
        Time.timeScale = 0f;
        InputBlocker.Block();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
        InputBlocker.Unblock();
    }

    public void OnButtonClicked()
    {
        InGameUIManager.Instance.HideEndingUI();
        SceneManager.LoadScene("Assets/Scenes/MainUI.unity");
        Debug.Log("메인 메뉴로 버튼 미구현");
    }

    public void SetRecordValue(GameResult gameResult)
    {
        if(title != null)
            title.text = gameResult.IsClear ? "Clear" : "Game Over";

        if (playTime != null)
            playTime.text = FormatTime(gameResult.PlayTimeSeconds);

        if (levelAchieved != null)
            levelAchieved.text = $"Lv. {gameResult.LevelAchieved}";

        if (monsterKills != null)
            monsterKills.text = $"{gameResult.MonsterKills}";
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
