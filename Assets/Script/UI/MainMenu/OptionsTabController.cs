using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsTabController : MonoBehaviour
{
    [Header("Canvas")]
    public GameObject optionsCanvas;
    public GameObject mainCanvas;

    [Header("Panel")]
    public GameObject gamePanel;
    public GameObject controlsPanel;
    public GameObject videoPanel;

    public void ShowMainCanvas()
    {
        if (mainCanvas != null)
            mainCanvas.SetActive(true);

        if (optionsCanvas != null)
            optionsCanvas.SetActive(false);        
    }

    public void ShowGamePanel()
    {
        SetAllPanelsInactive();
        if (gamePanel != null)
            gamePanel.SetActive(true);
    }

    public void ShowControlsPanel()
    {
        SetAllPanelsInactive();
        if (controlsPanel != null)
            controlsPanel.SetActive(true);
    }

    public void ShowVideoPanel()
    {
        SetAllPanelsInactive();
        if (videoPanel != null)
            videoPanel.SetActive(true);
    }

    void SetAllPanelsInactive()
    {
        if (gamePanel != null) gamePanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (videoPanel != null) videoPanel.SetActive(false);
    }
}
