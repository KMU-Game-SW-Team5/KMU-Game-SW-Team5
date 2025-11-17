using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class VideoSettingsPanel : MonoBehaviour
{
    [SerializeField] private Button btnFullscreenOn, btnFullscreenOff;
    [SerializeField] private Button btnVsyncOn, btnVsyncOff;
    [SerializeField] private Slider sliderBrightness;
    [SerializeField] private TMP_Dropdown dropdownResolution;

    private Resolution[] available;

    void OnEnable() { BuildResolutions(); Refresh(); }

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
        SettingsService.Brightness = v;
    }

    public void OnChangeResolution(int idx)
    {
        if (available == null || idx < 0 || idx >= available.Length) return;
        var r = available[idx];
        SettingsService.SetResolution(r.width, r.height, SettingsService.FullScreen);
    }

    // 드롭다운 해상도 목록 생성 및 현재 해상도에 맞춰 선택을 맞추는 함수
    private void BuildResolutions()
    {
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
        sliderBrightness.SetValueWithoutNotify(SettingsService.Brightness);
        RefreshFullScreenUI();
        RefreshVSyncOnUI();
    }

    private void RefreshFullScreenUI()
    {
        bool on = SettingsService.FullScreen;
        SetSelected(btnFullscreenOn, on);
        SetSelected(btnFullscreenOff, !on);
    }

    private void RefreshVSyncOnUI()
    {
        bool on = SettingsService.VSyncOn;
        SetSelected(btnVsyncOn, on);
        SetSelected(btnVsyncOff, !on);
    }

    private void SetSelected(Button b, bool on)
    {
        if (b == null) return;
        if (on) b.GetComponentInChildren<TextMeshProUGUI>(true).color = new Color(1f, 0.7f, 0.3f);
        else b.GetComponentInChildren<TextMeshProUGUI>(true).color = new Color(1f, 1f, 1f);


        var selectable = b.GetComponent<UnityEngine.UI.Selectable>();
        if (selectable != null)
            selectable.interactable = !on;
    }
}
