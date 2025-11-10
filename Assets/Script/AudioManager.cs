using UnityEngine;

[DefaultExecutionOrder(-1000)]  // 다른 스크립트보다 우선 순위 높게 설정
public class AudioManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SettingsService.OnMasterVolumeChanged += ApplyMasterVolume;
        SettingsService.ApplyAll();        
    }

    private void OnDestroy()
    {
        SettingsService.OnMasterVolumeChanged -= ApplyMasterVolume;
    }

    void ApplyMasterVolume(float v)
    {
        AudioListener.volume = v;
    }
}
