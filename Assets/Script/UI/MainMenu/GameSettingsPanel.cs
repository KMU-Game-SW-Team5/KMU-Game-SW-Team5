using TMPro;
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
        if (sliderSoundVolume != null)
            sliderSoundVolume.SetValueWithoutNotify(SettingsService.MasterVolume);

        // 각 섹션별로 관련 UI가 존재할 때만 갱신
        if (btnEasy != null || btnNormal != null || btnHard != null)
            RefreshDifficultyUI();

        if (btnTooltipsOn != null || btnTooltipsOff != null)
            RefreshTooltipsUI();
    }

    private void RefreshDifficultyUI()
    {
        int d = SettingsService.GameDifficulty;
        if (btnEasy != null) SetSelected(btnEasy, d == 0);
        if (btnNormal != null) SetSelected(btnNormal, d == 1);
        if (btnHard != null) SetSelected(btnHard, d == 2);
    }

    private void RefreshTooltipsUI()
    {
        bool on = SettingsService.Tooltips;
        if (btnTooltipsOn != null) SetSelected(btnTooltipsOn, on);
        if (btnTooltipsOff != null) SetSelected(btnTooltipsOff, !on);
    }

    private void SetSelected(Button b, bool on)
    {
        if (b == null) return;

        TextMeshProUGUI targetText = b.GetComponentInChildren<TextMeshProUGUI>(true);
        if (targetText == null) return;

        float currentAlpha = targetText.color.a;

        if (on) targetText.color = new Color(1f, 0.7f, 0.3f, currentAlpha);
        else targetText.color = new Color(1f, 1f, 1f, currentAlpha);

        var selectable = b.GetComponent<UnityEngine.UI.Selectable>();
        if (selectable != null)
            selectable.interactable = !on;
    }
}