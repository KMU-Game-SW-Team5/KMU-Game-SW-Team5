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
    bool isDuplicateActive;         // 이 액티브 카드가 중복 업그레이드 카드인지

    private int numOfStar;

    public int Level => numOfStar;

    private void OnEnable()
    {
        InitCard();
        DrawSkillCard();
        SetCardStar();
    }

    // Active, Passive 카드 중 랜덤 선택
    private void DrawSkillCard()
    {
        // 보유한 액티브 스킬 개수가 3개 이하이고, 액티브 스킬을 뽑을 확률에 들어갈 때 true
        isActive = UnityEngine.Random.Range(0, 100) <= activeSKillPercent;

        if (isActive)
        {
            selectedActiveSkill = DrawActiveSkill();
            if (selectedActiveSkill == null)
            {
                Debug.Log("selected Active skill is null");
            }
        }
        else
        {
            selectedPassiveSkill = DrawPassiveSkill();
        }
    }

    private ActiveSkillBase DrawActiveSkill()
    {
        bool duplicate;
        ActiveSkillBase activeSkill = SkillManager.Instance.PreviewActiveSkillAutoFromDeck(out duplicate);

        if (activeSkill == null)
        {
            Debug.Log("Active skill is null");
            return null;
        }

        selectedActiveSkill = activeSkill;
        isDuplicateActive = duplicate;

        skillName.text = activeSkill.GetSkillName();

        if (duplicate)
        {
            // 🔹 중복 카드라면, "강화 후 레벨"을 미리 보여주기 위해 +1 해서 그림
            numOfStar = activeSkill.GetNumOfStar() + 1;
        }
        else
        {
            // 🔹 신규 카드라면 "처음 레벨" (기획에 맞게 0 또는 1 선택)
            // 기존 ClearStar 후 레벨이 0이었다면 0으로 두고,
            // 처음부터 1레벨로 보여주고 싶으면 1로 두면 됨.
            numOfStar = 0;
        }

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
        numOfStar = 0;
        icon.sprite = passiveSkill.GetIcon();
        description.text = passiveSkill.GetSkillDescription();

        return passiveSkill;
    }

    // 레벨 반영하여 카드에 별 배치
    private void SetCardStar()
    {
        for (int i = 0; i < numOfStar; i++)
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
            Debug.Log(selectedActiveSkill + " was selected");

            // ✅ 여기서 덱 제거 / 이동 / 별 조정까지 한 번에 처리
            SkillManager.Instance.CommitActiveSkillSelection(selectedActiveSkill);
        }
        else
        {
            Debug.Log(selectedPassiveSkill + " was selected");
            SkillManager.Instance.AddPassiveSkill(selectedPassiveSkill);
            // 패시브도 나중에 필요하면 별/중복 처리 로직 분리 가능
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
