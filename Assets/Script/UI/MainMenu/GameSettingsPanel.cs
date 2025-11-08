using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class GameSettingsPanel : MonoBehaviour
{
    [SerializeField] private Button btnEasy, btnNormal, btnHard;
    [SerializeField] private Slider sliderSoundVolume;
    [SerializeField] private Button btnTooltipsOn, btnTooltipsOff;

    void OnEnable() { Refresh(); }

    public void OnClickDifficulty(int idx)
    {
        SettingsService.GameDifficulty = idx; 
        RefreshDifficultyUI(); 
        SettingsService.Save();
    }

    public void OnChangeVolume(float v)
    {
        SettingsService.MasterVolume = v; 
        SettingsService.Save();
    }

    public void OnClickTooltips(bool on)
    {
        SettingsService.Tooltips = on; 
        RefreshTooltipsUI(); 
        SettingsService.Save();
    }

    private void Refresh()
    {
        sliderSoundVolume.SetValueWithoutNotify(SettingsService.MasterVolume);
        RefreshDifficultyUI();
        RefreshTooltipsUI();
    }

    private void RefreshDifficultyUI()
    {
        int d = SettingsService.GameDifficulty;
        SetSelected(btnEasy, d == 0);
        SetSelected(btnNormal, d == 1);
        SetSelected(btnHard, d == 2);
    }

    private void RefreshTooltipsUI()
    {
        bool on = SettingsService.Tooltips;
        SetSelected(btnTooltipsOn, on);
        SetSelected(btnTooltipsOff, !on);
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