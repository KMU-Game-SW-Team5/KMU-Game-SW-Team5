using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public static class SettingsService
{
    public static event System.Action<float> OnMasterVolumeChanged;
    public static event System.Action<float> OnBrightnessChanged;

    // -----------------------------
    // GAME SETTINGS
    // -----------------------------
    public static int GameDifficulty
    {
        get => PlayerPrefs.GetInt("game.difficulty", 1);
        set { PlayerPrefs.SetInt("game.difficulty", value); }
    }

    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat("audio.volume", 0.8f); 
        set
        {
            float v = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("audio.volume", v);
            OnMasterVolumeChanged?.Invoke(v);   // 변경 알림
        }
    }

    public static bool Tooltips
    {
        get => PlayerPrefs.GetInt("ui.tooltips", 1) == 1; 
        set { PlayerPrefs.SetInt("ui.tooltips", value ? 1 : 0); }
    }

    // -----------------------------
    // CONTROLS SETTINGS
    // -----------------------------
    public static bool InvertMouse
    {
        get => PlayerPrefs.GetInt("controls.invert", 0) == 1; 
        set { PlayerPrefs.SetInt("controls.invert", value ? 1 : 0); }
    }

    public static float MouseSensitivity
    {
        get => PlayerPrefs.GetFloat("controls.mouseSens", 1.0f); 
        set { PlayerPrefs.SetFloat("controls.mouseSens", Mathf.Clamp(value, 0.1f, 10f)); }
    }

    public static float CameraSensitivity
    {
        get => PlayerPrefs.GetFloat("controls.camSens", 1.0f); 
        set { PlayerPrefs.SetFloat("controls.camSens", Mathf.Clamp(value, 0.1f, 10f)); }
    }

    // -----------------------------
    // VIDEO SETTINGS
    // -----------------------------
    public static bool FullScreen
    {
        get => PlayerPrefs.GetInt("video.fullscreen", 1) == 1; 
        set
        {
            PlayerPrefs.SetInt("video.fullscreen", value ? 1 : 0);
            Screen.fullScreen = value;
        }
    }

    public static bool VSyncOn
    {
        get => PlayerPrefs.GetInt("video.vsync", 1) == 1; 
        set
        {
            PlayerPrefs.SetInt("video.vsync", value ? 1 : 0); 
            QualitySettings.vSyncCount = value ? 1 : 0;
        }
    }

    public static float Brightness
    {
        get => PlayerPrefs.GetFloat("video.brightness", 0.5f); 
        set
        {
            float v = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("video.brightness", v);
            OnBrightnessChanged?.Invoke(v);   // 변경 알림
        }
    }

    public static void SetResolution(int width, int height, bool fullscreen)
    {
        Screen.SetResolution(width, height, fullscreen);
        PlayerPrefs.SetInt("video.res.w", width);
        PlayerPrefs.SetInt("video.res.h", height);
    }

    public static void ApplySavedResolution()
    {
        int w = PlayerPrefs.GetInt("video.res.w", Screen.currentResolution.width);
        int h = PlayerPrefs.GetInt("video.res.h", Screen.currentResolution.height);
        Screen.SetResolution(w, h, FullScreen);
    }

    // -----------------------------
    // UTILS
    // -----------------------------
    public static void Save() => PlayerPrefs.Save();

    public static void ApplyAll()
    {
        // PlayerPrefs는 게임이 종료되어도 저장된 정보가 삭제되지 않음
        // 그러나, 실제 게임 적용은 삭제되기 때문에 적용을 일괄적으로 할 필요 있음.

        // 수직 동기화 적용
        QualitySettings.vSyncCount = VSyncOn ? 1 : 0;

        // 해상도 및 전체화면 OnOff 적용
        int width = PlayerPrefs.GetInt("width", Screen.currentResolution.width);
        int height = PlayerPrefs.GetInt("height", Screen.currentResolution.height);
        Screen.SetResolution(width, height, FullScreen);

        // 오디오 적용
        OnMasterVolumeChanged?.Invoke(MasterVolume);

        // 밝기 적용
        OnBrightnessChanged?.Invoke(Brightness);
    }
}
