using TMPro;
using UnityEditor;
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
        SettingsService.Save(); 
        OnEnable();
    }

    public void OnChangeMouseSensitivity(float v)
    {
        SettingsService.MouseSensitivity = v; 
        SettingsService.Save();
    }

    public void OnChangeCameraSensitivity(float v)
    {
        SettingsService.CameraSensitivity = v; 
        SettingsService.Save();
    }

    private void Refresh()
    {
        sliderMouseSensitivity.SetValueWithoutNotify(SettingsService.MouseSensitivity);
        slidercameraSensitivity.SetValueWithoutNotify(SettingsService.CameraSensitivity);
        RefreshInvertMouseUI();
    }

    private void RefreshInvertMouseUI()
    {
        bool inv = SettingsService.InvertMouse;
        SetSelected(btnInvertMouseOn, inv);
        SetSelected(btnInvertMouseOff, !inv);
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
