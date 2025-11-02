using System.Collections.Generic;
using UnityEngine;

public class OptionsButtonGroup : MonoBehaviour
{
    [SerializeField] private List<OptionsButton> buttons = new List<OptionsButton>();

    [Header("기본으로 선택될 버튼")]
    [SerializeField] private OptionsButton defaultSelectedButton;

    void Start()
    {
        // 시작 시 기본 버튼 선택
        if (defaultSelectedButton != null)
            OnButtonSelected(defaultSelectedButton);
    }

    public void Register(OptionsButton button)
    {
        if (!buttons.Contains(button))
            buttons.Add(button);
    }

    public void OnButtonSelected(OptionsButton selected)
    {
        foreach (var b in buttons)
        {
            b.SetSelectedImage(false);
        }

        selected.SetSelectedImage(true);
    }
}
