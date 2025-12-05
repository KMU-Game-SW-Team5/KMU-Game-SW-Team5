using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-1000)]  // 다른 스크립트보다 우선 순위 높게 설정
public class VideoManager : MonoBehaviour
{
    // SettingsService가 모든 설정을 broadcast하고, 해당 클래스는 리스너 역할만 수행
    // 따라서 싱글톤 패턴으로 존재할 필요 없음 (251118 기준)
    private static bool _created = false;

    private static bool _baselineReady = false;
    private static Color _ambientBaseline; // 초기 기준

    void Awake()
    {
        if (_created)
        {
            Destroy(gameObject);
            return;
        }
        _created = true;

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
        float maxF = 1f;

        float factor = PivotMap(t, mid, eL: 2.2f, eR: 0.6f, minF, maxF);

        Debug.Log($"[VideoManager] ApplyBrightness t={t} factor={factor} ambientMode={RenderSettings.ambientMode}");

        // === 테스트용: 강제로 Flat 모드로 바꿔서 ambientLight를 직접 적용 ===
        // 실제 게임에서는 포스트프로세싱/스카이박스 처리 방식을 고려해 적용 방법 선택하세요.
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(factor, factor, factor, 1f);

        // (선택) directional light가 너무 밝아서 묻히면 강제로 조정해볼 수 있음:
        // Light dir = FindObjectOfType<Light>(); if (dir != null && dir.type == LightType.Directional) dir.intensity = Mathf.Clamp(factor * 2f, 0f, 2f);
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