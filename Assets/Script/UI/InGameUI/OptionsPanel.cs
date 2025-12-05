using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsPanel : MonoBehaviour
{
    [Header("Panel")]
    public GameObject gamePanel;
    public GameObject controlsPanel;
    public GameObject videoPanel;

    private void OnEnable()
    {
        Time.timeScale = 0f;
        InputBlocker.Block();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        // 패널 닫기 전에 Controls UI 값(슬라이더)을 SettingsService로 강제 푸시
        if (controlsPanel != null)
        {
            var ctrl = controlsPanel.GetComponentInChildren<ControlsSettingsPanel>(true);
            if (ctrl != null)
            {
                ctrl.PushUIValuesToSettings();
            }
        }

        // 모든 리스너에 현재 저장값 재전파
        SettingsService.ApplyAll();

        // 다른 '일시정지' UI가 활성인지 확인 (예: LevelUpUI)
        var levelUp = FindObjectOfType<LevelUpUI>(true);
        bool isLevelUpActive = levelUp != null && levelUp.gameObject.activeInHierarchy;

        if (!isLevelUpActive)
        {
            // 레벨업 등 다른 일시정지 UI가 없다면 정상적으로 시간/입력 복구
            Time.timeScale = 1f;
            InputBlocker.Unblock();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // 레벨업 UI가 활성 상태이면 Time 및 입력 상태를 유지하도록 아무 것도 하지 않음.
            // (레벨업 UI의 OnDisable에서 복구가 이루어질 것임)
            Debug.Log("[OptionsPanel] Keeping time/input blocked because LevelUpUI is active.");
        }
    }

    public void OnClickGameBtn()
    {
        SetAllPanelsInactive();
        gamePanel?.SetActive(true);
    }

    public void OnClickControlsBtn()
    {
        SetAllPanelsInactive();
        controlsPanel?.SetActive(true);
    }

    public void OnClickVideoBtn()
    {
        SetAllPanelsInactive();
        videoPanel?.SetActive(true);
    }

    public void OnClickReturnToGame()
    {
        gameObject?.SetActive(false);
    }

    public void OnClickReturnToMainMenu()
    {
        gameObject?.SetActive(false);
        if (Application.CanStreamedLevelBeLoaded("Assets/Scenes/MainUI.unity"))
            SceneManager.LoadScene("Assets/Scenes/MainUI.unity");
        else
            Debug.LogWarning("Assets/Scenes/MainUI.unity is not in Build Settings");
    }

    void SetAllPanelsInactive()
    {
        gamePanel?.SetActive(false);
        controlsPanel?.SetActive(false);
        videoPanel?.SetActive(false);
    }
}