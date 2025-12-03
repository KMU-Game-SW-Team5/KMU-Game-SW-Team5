using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneLoaderWithFade : MonoBehaviour
{
    [Header("Fade UI")]
    public CanvasGroup fadeCanvasGroup; // 암전 UI (검은 배경을 가진 CanvasGroup)
    public float fadeDuration = 1f;     // 암전 효과 지속 시간

    //void Start()
    //{
    //    // 씬 로딩 시작 시 암전 시작
    //    StartCoroutine(LoadSceneWithFade("MapMakerScene"));
    //}

    public IEnumerator LoadSceneWithFade(string sceneName)
    {
        // 암전 시작 (알파값을 1로 설정)
        yield return StartCoroutine(Fade(1));

        // 씬을 비동기적으로 로드
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);

        // 씬 로드가 완료될 때까지 기다림
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // 로드 완료 후 암전 해제 (알파값을 0으로 설정)
        yield return StartCoroutine(Fade(0));
    }

    // 페이드 효과 함수 (알파값을 변경)
    private IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha; // 정확한 값을 설정
    }
}
