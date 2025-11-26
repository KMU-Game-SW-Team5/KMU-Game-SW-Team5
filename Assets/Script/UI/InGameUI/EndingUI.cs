using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI playTime;
    [SerializeField] TextMeshProUGUI levelAchieved;
    [SerializeField] TextMeshProUGUI monsterKills;

    [SerializeField] Button btn_ReturnToMainMenu;
    [SerializeField] Button btn_OpenNicknamePanel;

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

    public void SetupClear()
    {
        btn_ReturnToMainMenu.gameObject.SetActive(false);
        btn_OpenNicknamePanel.gameObject.SetActive(true);

        if (title != null)
            title.text = "Clear";
    }

    public void SetupGameOver()
    {
        btn_OpenNicknamePanel.gameObject.SetActive(false);
        btn_ReturnToMainMenu.gameObject.SetActive(true);

        if (title != null)
            title.text = "Game Over";
    }

    public void OnClickReturnToMainMenu()
    {
        InGameUIManager.Instance.HideEndingUI();
        SceneManager.LoadScene("Assets/Scenes/MainUI.unity");
        Debug.Log("메인 메뉴로 버튼 미구현");
    }

    public void OnClickOpenNicknamePanel()
    {
        InGameUIManager.Instance.ShowEnterNicknamePanel();
    }

    public void SetValue(GameResult gameResult)
    {
        if (playTime != null)
            playTime.text = FormatTime(gameResult.PlayTime);

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
