using System.Collections.Generic;
using UnityEngine;

public class TabSelectionGroup : MonoBehaviour
{
    [SerializeField] private List<HoverSelectedVisual> buttons = new List<HoverSelectedVisual>();

    [Header("기본으로 선택될 버튼")]
    [SerializeField] private HoverSelectedVisual defaultSelectedButton;

    private HoverSelectedVisual currentSelected;

    void Start()
    {
        // 시작 시 기본 버튼 선택
        if (defaultSelectedButton != null)
            OnButtonSelected(defaultSelectedButton);
    }

    public void Register(HoverSelectedVisual button)
    {
        if (!buttons.Contains(button))
            buttons.Add(button);
    }

    public void OnButtonSelected(HoverSelectedVisual selected)
    {
        foreach (var b in buttons)
            b.SetSelected(false);

        selected.SetSelected(true);
        currentSelected = selected;
    }

    public HoverSelectedVisual GetCurrent() => currentSelected;
}
