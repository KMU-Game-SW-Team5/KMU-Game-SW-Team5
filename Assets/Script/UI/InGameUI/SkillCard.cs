using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private GameObject stars;
    [SerializeField] private GameObject starPrefab;

    private SkillData data;
    private int level;

    public SkillData Data => data;
    public int Level => level;

    public Action<SkillCard> OnSelected;

    public void SetSkill(SkillData option, Action<SkillCard> onSelected)
    {
        InitCard();

        data = option;
        level = option.level;
        OnSelected = onSelected;

        //icon.sprite = options.icon;
        description.text = option.description;

        for (int i = 0; i < option.level; i++)
        {
            Instantiate(starPrefab, stars.transform);
        }
    }

    public void OnClickedCard()
    {
        gameObject.transform.localScale = new Vector3(1, 1, 1);
        OnSelected?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        gameObject.transform.localScale = new Vector3(1.1f, 1.1f, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        gameObject.transform.localScale = new Vector3(1, 1, 1);
    }

    private void InitCard()
    {
        icon.sprite = null;
        description.text = "";
        ClearStars();
    }

    private void ClearStars()
    {
        foreach (Transform star in stars.transform)
        {
            Destroy(star.gameObject);
        }
    }
}
