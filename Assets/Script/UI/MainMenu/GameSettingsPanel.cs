using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettingsPanel : MonoBehaviour
{
    [SerializeField] private Button btnEasy, btnNormal, btnHard;
    [SerializeField] private Slider sliderSoundVolume;
    [SerializeField] private Button btnTooltipsOn, btnTooltipsOff;

    void OnEnable() { Refresh(); }

    public void OnClickDifficulty(int idx)
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        if (currentSceneName != "MainUI") return;

        SettingsService.GameDifficulty = idx; 
        RefreshDifficultyUI(); 
    }

    public void OnChangeVolume(float v)
    {
        SettingsService.MasterVolume = v; 
    }

    public void OnClickTooltips(bool on)
    {
        SettingsService.Tooltips = on; 
        RefreshTooltipsUI(); 
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
        TextMeshProUGUI targetText = b.GetComponentInChildren<TextMeshProUGUI>(true);
        float currentAlpha = targetText.color.a;

        if (b == null) return;
        if (on) targetText.color = new Color(1f, 0.7f, 0.3f, currentAlpha);
        else targetText.color = new Color(1f, 1f, 1f, currentAlpha);


        var selectable = b.GetComponent<UnityEngine.UI.Selectable>();
        if (selectable != null)
            selectable.interactable = !on;
    }
}