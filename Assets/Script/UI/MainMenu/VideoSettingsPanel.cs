using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VideoSettingsPanel : MonoBehaviour
{
    [SerializeField] private Button btnFullscreenOn, btnFullscreenOff;
    [SerializeField] private Button btnVsyncOn, btnVsyncOff;
    [SerializeField] private Slider sliderBrightness;
    [SerializeField] private TMP_Dropdown dropdownResolution;

    private Resolution[] available;

    void OnEnable()
    {
        if (dropdownResolution != null) BuildResolutions();
        Refresh();

        // EventSystem 존재 확인
        if (EventSystem.current == null)
        {
            Debug.LogWarning("[VideoSettingsPanel] EventSystem not found in scene. UI events won't work without it.");
        }

        // 런타임 구독: Inspector 바인딩이 빠졌을 때 대비
        if (sliderBrightness != null)
        {
            // 중복 구독 방지
            sliderBrightness.onValueChanged.RemoveListener(OnChangeBrightness);
            sliderBrightness.onValueChanged.AddListener(OnChangeBrightness);
        }
    }

    void OnDisable()
    {
        if (sliderBrightness != null)
        {
            sliderBrightness.onValueChanged.RemoveListener(OnChangeBrightness);
        }
    }

    public void OnClickFullScreen(bool on)
    {
        SettingsService.FullScreen = on;
        RefreshFullScreenUI();
    }

    public void OnClickVSync(bool on)
    {
        SettingsService.VSyncOn = on;
        RefreshVSyncOnUI();
    }

    public void OnChangeBrightness(float v)
    {
        Debug.Log($"[VideoSettingsPanel] OnChangeBrightness called: {v}");
        SettingsService.Brightness = v;
    }

    public void OnChangeResolution(int idx)
    {
        if (dropdownResolution == null) return;
        if (available == null || idx < 0 || idx >= available.Length) return;
        var r = available[idx];
        SettingsService.SetResolution(r.width, r.height, SettingsService.FullScreen);
    }

    private void BuildResolutions()
    {
        if (dropdownResolution == null) return;

        dropdownResolution.ClearOptions();
        available = Screen.resolutions;
        var options = new System.Collections.Generic.List<string>();
        int currentIndex = 0;

        for (int i = 0; i < available.Length; i++)
        {
            var r = available[i];
            string label = $"{r.width} x {r.height}";
            if (!options.Contains(label)) options.Add(label);
            if (r.width == Screen.currentResolution.width && r.height == Screen.currentResolution.height)
                currentIndex = i;
        }
        dropdownResolution.AddOptions(options);
        dropdownResolution.SetValueWithoutNotify(currentIndex);
    }

    private void Refresh()
    {
        if (sliderBrightness != null)
            sliderBrightness.SetValueWithoutNotify(SettingsService.Brightness);

        RefreshFullScreenUI();
        RefreshVSyncOnUI();
    }

    private void RefreshFullScreenUI()
    {
        bool on = SettingsService.FullScreen;
        if (btnFullscreenOn != null) SetSelected(btnFullscreenOn, on);
        if (btnFullscreenOff != null) SetSelected(btnFullscreenOff, !on);
    }

    private void RefreshVSyncOnUI()
    {
        bool on = SettingsService.VSyncOn;
        if (btnVsyncOn != null) SetSelected(btnVsyncOn, on);
        if (btnVsyncOff != null) SetSelected(btnVsyncOff, !on);
    }

    private void SetSelected(Button b, bool on)
    {
        if (b == null) return;
        if (on) b.GetComponentInChildren<TextMeshProUGUI>(true).color = new Color(1f, 0.7f, 0.3f);
        else b.GetComponentInChildren<TextMeshProUGUI>(true).color = new Color(1f, 1f, 1f);
    }
}