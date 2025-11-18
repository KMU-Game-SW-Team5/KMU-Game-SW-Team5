using UnityEngine;

[DefaultExecutionOrder(-1000)]  // 다른 스크립트보다 우선 순위 높게 설정
public class AudioManager : MonoBehaviour
{
    // SettingsService가 모든 설정을 broadcast하고, 해당 클래스는 리스너 역할만 수행
    // 따라서 싱글톤 패턴으로 존재할 필요 없음 (251118 기준)
    private static bool _created = false;

    private void Awake()
    {
        if (_created)
        {
            Destroy(gameObject);
            return;
        }
        _created = true;

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
