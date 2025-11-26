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
    [SerializeField] private TextMeshProUGUI skillName;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private GameObject stars;
    [SerializeField] private GameObject starPrefab;

    [Header("뽑기 설정")]
    [SerializeField][Range(0, 100)] private int activeSKillPercent = 20;

    // 뽑은 카드
    ActiveSkillBase selectedActiveSkill;
    PassiveSkillBase selectedPassiveSkill;
    bool isActive;      // 뽑은 게 액티브 스킬인지

    private int level;

    public int Level => level;

    private void OnEnable()
    {
        InitCard();
        DrawSkillCard();
        SetCardStar();
    }

    // Active, Passive 카드 중 랜덤 선택
    private void DrawSkillCard()
    {
        isActive = UnityEngine.Random.Range(0, 100) <= activeSKillPercent;

        if (isActive)
        {
            selectedActiveSkill = DrawActiveSkill();
        }
        else
        {
            selectedPassiveSkill = DrawPassiveSkill();
        }
    }

    private ActiveSkillBase DrawActiveSkill()
    {
        ActiveSkillBase activeSkill = SkillManager.Instance.DrawActiveSkillAutoFromDeck();

        if (activeSkill == null)
        {
            Debug.Log("Active skill is null");

            return null;
        }
        skillName.text = activeSkill.GetSkillName();
        level = activeSkill.GetNumOfStar();
        icon.sprite = activeSkill.GetIcon();
        description.text = activeSkill.GetAcquisitionDescriptionPlain();
        
        return activeSkill;
    }

    private PassiveSkillBase DrawPassiveSkill()
    {
        PassiveSkillBase passiveSkill = SkillManager.Instance.DrawPassiveSkillFromDeck();

        if (passiveSkill == null)
        {
            Debug.Log("Passive skill is null");

            return null;
        }

        skillName.text = passiveSkill.GetSkillName();
        level = 0;
        icon.sprite = passiveSkill.GetIcon();
        description.text = passiveSkill.GetSkillDescription();

        return passiveSkill;
    }

    // 레벨 반영하여 카드에 별 배치
    private void SetCardStar()
    {
        for (int i = 0; i < level; i++)
        {
            Instantiate(starPrefab, stars.transform);
        }
    }

    public void OnClickedCard()
    {
        gameObject.transform.localScale = new Vector3(1, 1, 1);

        // 카드 변경 사항 적용
        if (isActive)
        {
            Debug.Log(selectedActiveSkill.ToString() + "was selected");
            SkillManager.Instance.AddActiveSkill(selectedActiveSkill);
        }
        else
        {
            Debug.Log(selectedPassiveSkill.ToString() + "was selected");
            SkillManager.Instance.AddPassiveSkill(selectedPassiveSkill);
        }

        gameObject.transform.parent.GetComponent<LevelUpUI>().CloseSkillChoiceUI();
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
        skillName.text = "";
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

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
