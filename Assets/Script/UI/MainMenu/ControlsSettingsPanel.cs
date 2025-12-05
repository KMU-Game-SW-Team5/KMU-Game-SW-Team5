using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettingsPanel : MonoBehaviour
{
    [SerializeField] private Button btnInvertMouseOn, btnInvertMouseOff;
    [SerializeField] private Slider sliderMouseSensitivity;
    [SerializeField] private Slider slidercameraSensitivity;

    // UI 표시 범위: 0.2 ~ 3.0 (디자이너용)
    private const float UISensMin = 0.2f;
    private const float UISensMax = 3.0f;

    [Header("조정 옵션")]
    [Tooltip("슬라이더 값에 곱해지는 글로벌 감도 배율. 예: 0.5면 슬라이더 값의 절반이 실제 적용됩니다.")]
    [SerializeField] private float sensitivityMultiplier = 1f;

    void OnEnable()
    {
        if (sliderMouseSensitivity != null)
        {
            sliderMouseSensitivity.minValue = UISensMin;
            sliderMouseSensitivity.maxValue = UISensMax;
        }
        if (slidercameraSensitivity != null)
        {
            slidercameraSensitivity.minValue = UISensMin;
            slidercameraSensitivity.maxValue = UISensMax;
        }

        Refresh();

        if (sliderMouseSensitivity != null)
        {
            sliderMouseSensitivity.onValueChanged.RemoveListener(OnChangeMouseSensitivity);
            sliderMouseSensitivity.onValueChanged.AddListener(OnChangeMouseSensitivity);
        }

        if (slidercameraSensitivity != null)
        {
            slidercameraSensitivity.onValueChanged.RemoveListener(OnChangeCameraSensitivity);
            slidercameraSensitivity.onValueChanged.AddListener(OnChangeCameraSensitivity);
        }
    }

    void OnDisable()
    {
        if (sliderMouseSensitivity != null)
            sliderMouseSensitivity.onValueChanged.RemoveListener(OnChangeMouseSensitivity);
        if (slidercameraSensitivity != null)
            slidercameraSensitivity.onValueChanged.RemoveListener(OnChangeCameraSensitivity);
    }

    public void OnClickInvert(bool on)
    {
        SettingsService.InvertMouse = on;
        Refresh();
    }

    // 저장: slider value를 multiplier로 스케일한 값(final)을 그대로 SettingsService에 저장.
    public void OnChangeMouseSensitivity(float v)
    {
        float mult = Mathf.Max(0.0001f, sensitivityMultiplier);
        float final = v * mult;
        Debug.Log($"[ControlsSettingsPanel] OnChangeMouseSensitivity -> slider:{v} multiplier:{mult} final:{final}");
        SettingsService.MouseSensitivity = final;
    }

    public void OnChangeCameraSensitivity(float v)
    {
        float mult = Mathf.Max(0.0001f, sensitivityMultiplier);
        float final = v * mult;
        Debug.Log($"[ControlsSettingsPanel] OnChangeCameraSensitivity -> slider:{v} multiplier:{mult} final:{final}");
        SettingsService.CameraSensitivity = final;
    }

    // 새로 추가: UI에 보이는 슬라이더 값(수정 여부와 관계없이)을 SettingsService로 강제 푸시
    public void PushUIValuesToSettings()
    {
        float mult = Mathf.Max(0.0001f, sensitivityMultiplier);

        if (sliderMouseSensitivity != null)
        {
            float sliderVal = sliderMouseSensitivity.value;
            float final = sliderVal * mult;
            Debug.Log($"[ControlsSettingsPanel] PushUIValuesToSettings mouse: slider={sliderVal} mult={mult} final={final}");
            SettingsService.MouseSensitivity = final;
        }

        if (slidercameraSensitivity != null)
        {
            float sliderVal = slidercameraSensitivity.value;
            float final = sliderVal * mult;
            Debug.Log($"[ControlsSettingsPanel] PushUIValuesToSettings camera: slider={sliderVal} mult={mult} final={final}");
            SettingsService.CameraSensitivity = final;
        }
    }

    private void Refresh()
    {
        float mult = Mathf.Max(0.0001f, sensitivityMultiplier);

        if (sliderMouseSensitivity != null)
        {
            float stored = SettingsService.MouseSensitivity;
            float display = stored / mult;
            display = Mathf.Clamp(display, UISensMin, UISensMax);
            sliderMouseSensitivity.SetValueWithoutNotify(display);
        }

        if (slidercameraSensitivity != null)
        {
            float stored = SettingsService.CameraSensitivity;
            float display = stored / mult;
            display = Mathf.Clamp(display, UISensMin, UISensMax);
            slidercameraSensitivity.SetValueWithoutNotify(display);
        }

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