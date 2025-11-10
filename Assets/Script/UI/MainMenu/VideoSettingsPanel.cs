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

    void OnEnable() { Refresh(); }

    public void OnClickFullScreen(bool on)
    {
        SettingsService.FullScreen = on;
        SettingsService.ApplySavedResolution();
        SettingsService.Save();
        OnEnable();
    }

    public void OnClickVSync(bool on)
    {
        SettingsService.VSyncOn = on;
        SettingsService.Save();
        OnEnable();
    }

    public void OnChangeBrightness(float v)
    {
        SettingsService.Brightness = v;
        SettingsService.Save();
        // 실제 화면 노출(PostProcess) 연결은 프로젝트 셋업에 맞게 추가
    }

    public void OnChangeResolution(int idx)
    {
        if (available == null || idx < 0 || idx >= available.Length) return;
        var r = available[idx];
        SettingsService.SetResolution(r.width, r.height, SettingsService.FullScreen);
        SettingsService.Save();
    }

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
