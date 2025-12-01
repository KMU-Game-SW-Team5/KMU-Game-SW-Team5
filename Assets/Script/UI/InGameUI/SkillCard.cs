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

    // 🔹 이번 레벨업 뽑기에서 이미 나온 카드들 정보 공유 (중복 드로우 방지)
    private static HashSet<ActiveSkillBase> usedActiveSkills = new();
    private static HashSet<PassiveSkillBase> usedPassiveSkills = new();

    private void OnEnable()
    {
        InitCard();
        DrawSkillCard();
        SetCardStar();
    }

    // 드로우 때 나온 카드를 공유하기 위한 집합 초기화
    public static void BeginRollSession()
    {
        usedActiveSkills.Clear();
        usedPassiveSkills.Clear();
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
        const int maxTry = 20;

        ActiveSkillBase activeSkill = null;
        bool isDuplicate = false;   // 중복 강화 카드인지 여부 (이전에 만든 플래그)

        for (int i = 0; i < maxTry; i++)
        {
            activeSkill = SkillManager.Instance.PreviewActiveSkillAutoFromDeck(out isDuplicate);
            if (activeSkill == null)
                return null;

            // 이번 뽑기에서 아직 안 나온 카드면 사용
            if (!usedActiveSkills.Contains(activeSkill))
                break;

            // 이미 나온 카드면 null로 초기화하고 다시 시도
            activeSkill = null;
        }

        // 정말 후보 풀이 너무 적어서 전부 중복이면
        // 마지막 한 번은 그냥 허용 (무한 루프 방지용)
        if (activeSkill == null)
        {
            activeSkill = SkillManager.Instance.PreviewActiveSkillAutoFromDeck(out isDuplicate);
            if (activeSkill == null)
                return null;
        }

        // 🔹 이번 세션에서 사용한 카드 목록에 추가
        usedActiveSkills.Add(activeSkill);

        selectedActiveSkill = activeSkill;
        isDuplicateActive = isDuplicate;

        skillName.text = activeSkill.GetSkillName();

        // UI에 보일 별 개수 계산(중복이면 +1 해서 보여주기 등)
        if (isDuplicateActive)
            numOfStar = activeSkill.GetNumOfStar() + 1;
        else
            numOfStar = 0; // 신규 카드라면 0부터 등, 기획에 맞게

        icon.sprite = activeSkill.GetIcon();
        description.text = activeSkill.GetAcquisitionDescriptionPlain();

        return activeSkill;
    }



    private PassiveSkillBase DrawPassiveSkill()
    {
        const int maxTry = 20;

        PassiveSkillBase passiveSkill = null;

        for (int i = 0; i < maxTry; i++)
        {
            passiveSkill = SkillManager.Instance.DrawPassiveSkillFromDeck();
            if (passiveSkill == null)
                return null;

            if (!usedPassiveSkills.Contains(passiveSkill))
                break;

            passiveSkill = null;
        }

        if (passiveSkill == null)
        {
            passiveSkill = SkillManager.Instance.DrawPassiveSkillFromDeck();
            if (passiveSkill == null)
                return null;
        }

        usedPassiveSkills.Add(passiveSkill);

        selectedPassiveSkill = passiveSkill;

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
