using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)]  // 다른 스크립트보다 우선 순위 높게 설정
public class VideoManager : MonoBehaviour
{
    private static bool _baselineReady = false;
    private static Color _ambientBaseline; // 초기 기준

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        SettingsService.OnFullScreenChanged += ApplyFullScreen;
        SettingsService.OnVSyncChanged += ApplyVSync;
        SettingsService.OnBrightnessChanged += ApplyBrightness;
        SettingsService.OnResolutionChanged += ApplyResolution;

        InitBrightnessBaseline();

        SettingsService.ApplyAll();
    }

    private void OnDestroy()
    {
        SettingsService.OnFullScreenChanged -= ApplyFullScreen;
        SettingsService.OnVSyncChanged -= ApplyVSync;
        SettingsService.OnBrightnessChanged -= ApplyBrightness;        
        SettingsService.OnResolutionChanged -= ApplyResolution;
    }

    private void ApplyFullScreen(bool on)
    {
        Screen.fullScreen = on;
    }

    private void ApplyVSync(bool on)
    {
        QualitySettings.vSyncCount = on ? 1 : 0;
    }

    public void ApplyBrightness(float t)
    {
        InitBrightnessBaseline();

        float mid = _ambientBaseline.maxColorComponent;
        float minF = mid * 0.5f;
        float maxF = 1f;//Mathf.Min(1f, mid * 2f);

        // 중앙 피벗, 좌측은 천천히(지수>1), 우측은 가파르게(지수<1)
        float factor = PivotMap(t, mid, eL: 2.2f, eR: 0.6f, minF, maxF);

        RenderSettings.ambientLight = new Color(factor, factor, factor, 1f);
    }

    private void ApplyResolution(int w, int h, bool fullscreen)
    {
        Screen.SetResolution(w, h, fullscreen);
    }



    // t: 슬라이더(0~1), mid: 가운데에서 목표할 밝기(예: 0.01886797)
    // eL>1 이면 왼쪽이 "천천히" 0으로 내려감
    // eR<1 이면 오른쪽이 "가파르게" 1로 올라감
    private static float PivotMap(float t, float mid, float eL, float eR, float minF, float maxF)
    {
        t = Mathf.Clamp01(t);

        if (t <= 0.5f)
        {
            float u = (0.5f - t) / 0.5f;
            float k = Mathf.Pow(u, eL);
            return Mathf.Lerp(mid, minF, k);
        }
        else
        {
            float u = (t - 0.5f) / 0.5f;
            float k = Mathf.Pow(u, eR);
            return Mathf.Lerp(mid, maxF, k);
        }
    }

    public static void InitBrightnessBaseline()
    {
        if (_baselineReady) return;
        _ambientBaseline = RenderSettings.ambientLight; // 처음 한 번만
        _baselineReady = true;
    }
}
