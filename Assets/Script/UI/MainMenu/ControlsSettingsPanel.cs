using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettingsPanel : MonoBehaviour
{
    [SerializeField] private Button btnInvertMouseOn, btnInvertMouseOff;
    [SerializeField] private Slider sliderMouseSensitivity;
    [SerializeField] private Slider slidercameraSensitivity;

    void OnEnable() { Refresh(); }

    public void OnClickInvert(bool on)
    {
        SettingsService.InvertMouse = on;
        // SettingsService.Save(); // 필요하면 재활성화
        Refresh();
    }

    public void OnChangeMouseSensitivity(float v)
    {
        SettingsService.MouseSensitivity = v;
        // SettingsService.Save();
    }

    public void OnChangeCameraSensitivity(float v)
    {
        SettingsService.CameraSensitivity = v;
        // SettingsService.Save();
    }

    private void Refresh()
    {
        if (sliderMouseSensitivity != null)
            sliderMouseSensitivity.SetValueWithoutNotify(SettingsService.MouseSensitivity);

        if (slidercameraSensitivity != null)
            slidercameraSensitivity.SetValueWithoutNotify(SettingsService.CameraSensitivity);

        if (btnInvertMouseOn != null || btnInvertMouseOff != null)
            RefreshInvertMouseUI();
    }

    private void RefreshInvertMouseUI()
    {
        bool inv = SettingsService.InvertMouse;
        if (btnInvertMouseOn != null) SetSelected(btnInvertMouseOn, inv);
        if (btnInvertMouseOff != null) SetSelected(btnInvertMouseOff, !inv);
    }

    private void SetSelected(Button b, bool on)
    {
        if (b == null) return;
        var text = b.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text == null) return;

        if (on) text.color = new Color(1f, 0.7f, 0.3f);
        else text.color = new Color(1f, 1f, 1f);

        var selectable = b.GetComponent<UnityEngine.UI.Selectable>();
        if (selectable != null)
            selectable.interactable = !on;
    }
}