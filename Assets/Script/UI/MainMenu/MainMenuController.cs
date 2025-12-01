using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("옵션 메뉴 캔버스")]
    [SerializeField] private GameObject optionsCanvas;

    [Header("메인 메뉴 캔버스")]
    [SerializeField] private GameObject mainCanvas;

    [Header("메인 메뉴 패널")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject difficultyPanel;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void ShowDifficultyPanel()
    {
        difficultyPanel?.SetActive(true);
        menuPanel?.SetActive(false);
    }

    public void ShowMenuPanel()
    {
        menuPanel?.SetActive(true);
        difficultyPanel?.SetActive(false);
    }

    public void ShowOptionsCanvas()
    {
        optionsCanvas?.SetActive(true);
        mainCanvas?.SetActive(false);
    }
}
