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
        Time.timeScale = 1f;
        InputBlocker.Unblock();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
            Debug.LogWarning("Assets/Scenes/MainUI.unity가 Build Settings에 등록되어 있지 않습니다");
    }

    void SetAllPanelsInactive()
    {
        gamePanel?.SetActive(false);
        controlsPanel?.SetActive(false);
        videoPanel?.SetActive(false);
    }
}
