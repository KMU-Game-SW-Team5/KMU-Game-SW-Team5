using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultyPanel : MonoBehaviour
{
    [SerializeField] private MainMenuController mmc;

    public void OnClickDifficulty(int idx)
    {
        SettingsService.GameDifficulty = idx;

        StartGame();
    }

    public void OnClickBack() => mmc?.ShowMenuPanel();


    private void StartGame()
    {
        /*
        // GameScene이 빌드 세팅에 포함되어 있어야 함
        if (Application.CanStreamedLevelBeLoaded("GameScene"))
            SceneManager.LoadScene("GameScene");
        else
            Debug.LogWarning("GameScene이 Build Settings에 등록되어 있지 않습니다");
        */
        SceneManager.LoadScene("Assets/Scenes/MapMakerScene.unity");
    }
}
