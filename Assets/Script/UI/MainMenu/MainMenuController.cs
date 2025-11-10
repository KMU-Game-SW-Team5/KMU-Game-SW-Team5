using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("옵션 메뉴 캔버스")]
    public GameObject optionsCanvas;

    [Header("메인 메뉴 캔버스")]
    public GameObject mainCanvas;

    public void StartGame()
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

    public void ShowOptionsCanvas()
    {
        if (optionsCanvas != null) 
            optionsCanvas.SetActive(true);

        if (mainCanvas != null)
            mainCanvas.SetActive(false);
    }

    public void ExitGame()
    {
        Debug.Log("게임 종료 요청됨");

        // 에디터 환경에서는 종료되지 않으므로 조건 처리
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
