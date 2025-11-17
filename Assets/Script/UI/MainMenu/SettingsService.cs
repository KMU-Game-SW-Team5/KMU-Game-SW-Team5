using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public static class SettingsService
{
    public static event System.Action<int> OnGameDifficultyChanged;
    public static event System.Action<float> OnMasterVolumeChanged;
    public static event System.Action<bool> OnTooltipsChanged;

    public static event System.Action<bool> OnInvertMouseChanged;
    public static event System.Action<float> OnMouseSensitivityChanged;
    public static event System.Action<float> OnCameraSensitivityChanged;

    public static event System.Action<bool> OnFullScreenChanged;
    public static event System.Action<bool> OnVSyncChanged;
    public static event System.Action<float> OnBrightnessChanged;
    public static event System.Action<int, int, bool> OnResolutionChanged;

    // -----------------------------
    // GAME SETTINGS
    // -----------------------------
    public static int GameDifficulty
    {
        get => PlayerPrefs.GetInt("game.difficulty", 1);
        set
        {
            int v = Mathf.Clamp(value, 0, 2);
            if (v == GameDifficulty) return;
            PlayerPrefs.SetInt("game.difficulty", v);
            PlayerPrefs.Save();
            OnGameDifficultyChanged?.Invoke(v);
        }
    }

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat("audio.volume", 0.8f); 
        set
        {
            float v = Mathf.Clamp01(value);
            if (Mathf.Approximately(v, MasterVolume)) return;
            PlayerPrefs.SetFloat("audio.volume", v);
            PlayerPrefs.Save();
            OnMasterVolumeChanged?.Invoke(v);
        }
    }

    public static bool Tooltips
    {
        get => PlayerPrefs.GetInt("ui.tooltips", 1) == 1; 
        set
        {
            if (value == Tooltips) return;
            PlayerPrefs.SetInt("ui.tooltips", value ? 1 : 0);
            PlayerPrefs.Save();
            OnTooltipsChanged?.Invoke(value);
        }
    }

    // -----------------------------
    // CONTROLS SETTINGS
    // -----------------------------
    public static bool InvertMouse
    {
        get => PlayerPrefs.GetInt("controls.invert", 0) == 1; 
        set
        {
            if(value == InvertMouse) return;
            PlayerPrefs.SetInt("controls.invert", value ? 1 : 0);
            PlayerPrefs.Save();
            OnInvertMouseChanged?.Invoke(value);
        }
    }

    public static float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat("controls.mouseSens", 1.0f); 
        set
        {
            float v = Mathf.Clamp(value, 0.1f, 10f);
            if (Mathf.Approximately(v, MouseSensitivity     )) return;
            PlayerPrefs.SetFloat("controls.mouseSens", v);
            PlayerPrefs.Save();
            OnMouseSensitivityChanged?.Invoke(v);
        }
    }

    public static float CameraSensitivity
    {
        get => PlayerPrefs.GetFloat("controls.camSens", 1.0f); 
        set
        {
            float v = Mathf.Clamp(value, 0.1f, 10f);
            if (Mathf.Approximately(v, CameraSensitivity)) return;
            PlayerPrefs.SetFloat("controls.camSens", v);
            PlayerPrefs.Save();
            OnCameraSensitivityChanged?.Invoke(v);
        }
    }

    // -----------------------------
    // VIDEO SETTINGS
    // -----------------------------
    public static bool FullScreen
    {
        get => PlayerPrefs.GetInt("video.fullscreen", 1) == 1; 
        set
        {
            if(value == FullScreen) return;
            PlayerPrefs.SetInt("video.fullscreen", value ? 1 : 0);
            PlayerPrefs.Save();
            OnFullScreenChanged?.Invoke(value);
        }
    }

    public static bool VSyncOn
    {
        get => PlayerPrefs.GetInt("video.vsync", 1) == 1; 
        set
        {
            if(value == VSyncOn) return;
            PlayerPrefs.SetInt("video.vsync", value ? 1 : 0); 
            PlayerPrefs.Save();
            OnVSyncChanged?.Invoke(value);
        }
    }

    public static float Brightness
    {
        get => PlayerPrefs.GetFloat("video.brightness", 0.5f); 
        set
        {            
            float v = Mathf.Clamp01(value);
            if (Mathf.Approximately(v, Brightness)) return;
            PlayerPrefs.SetFloat("video.brightness", v);
            PlayerPrefs.Save();
            OnBrightnessChanged?.Invoke(v);   // 변경 알림
        }
    }

    public static void SetResolution(int width, int height, bool fullscreen)
    {
        PlayerPrefs.SetInt("video.res.w", width);
        PlayerPrefs.SetInt("video.res.h", height);
        PlayerPrefs.Save();
        OnResolutionChanged?.Invoke(width, height, fullscreen);
    }

    // -----------------------------
    // UTILS
    // -----------------------------
    public static void Save() => PlayerPrefs.Save();

    public static void ApplyAll()
    {
        // PlayerPrefs는 게임이 종료되어도 저장된 정보가 삭제되지 않음
        // 그러나, 실제 게임 적용은 삭제되기 때문에 적용을 일괄적으로 할 필요 있음.

        // Game
        OnGameDifficultyChanged?.Invoke(GameDifficulty);
        OnMasterVolumeChanged?.Invoke(MasterVolume);
        OnTooltipsChanged?.Invoke(Tooltips);

        // Controls
        OnInvertMouseChanged?.Invoke(InvertMouse);
        OnMouseSensitivityChanged?.Invoke(MouseSensitivity);
        OnCameraSensitivityChanged?.Invoke(CameraSensitivity);

        // Video 적용
        OnFullScreenChanged?.Invoke(FullScreen);
        OnVSyncChanged?.Invoke(VSyncOn);
        OnBrightnessChanged?.Invoke(Brightness);

        // 해상도도 이벤트로 통일 (아래에서 설명)
        OnResolutionChanged?.Invoke(
            PlayerPrefs.GetInt("video.res.w", Screen.currentResolution.width),
            PlayerPrefs.GetInt("video.res.h", Screen.currentResolution.height),
            FullScreen
        );
    }
}
