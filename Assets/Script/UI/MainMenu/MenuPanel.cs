using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] private MainMenuController mmc;

    public void OnClickStart() => mmc?.ShowDifficultyPanel();
    public void OnClickOptions() => mmc?.ShowOptionsCanvas();

    public void OnClickExit()
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
