using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void OnClickReturnBtn()
    {
        gameObject?.SetActive(false);
    }

    void SetAllPanelsInactive()
    {
        gamePanel?.SetActive(false);
        controlsPanel?.SetActive(false);
        videoPanel?.SetActive(false);
    }
}
