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
    [SerializeField][Range(0, 100)] private int activeSKillPercent = 40;

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

    // Active, Passive 카드 중 변동 확률로 선택
    private void DrawSkillCard()
    {
        var sm = SkillManager.Instance;
        if (sm == null)
        {
            Debug.LogError("SkillManager.Instance is null");
            return;
        }

        int equippedActiveCount = sm.GetNumOfActiveSkills();

        if (equippedActiveCount < 4)
        {
            // 🔹 4종 전까지: 인스펙터에서 설정한 확률 사용 (예: 30)
            bool tryActive = UnityEngine.Random.Range(0, 100) < activeSKillPercent;

            if (tryActive)
            {
                isActive = true;
                selectedActiveSkill = DrawActiveSkill();

                // 액티브 후보가 없거나, 전부 이번 세트에서 이미 나왔으면 패시브로 폴백
                if (selectedActiveSkill == null)
                {
                    isActive = false;
                    selectedPassiveSkill = DrawPassiveSkill();
                }
            }
            else
            {
                isActive = false;
                selectedPassiveSkill = DrawPassiveSkill();
            }
        }
        else
        {
            // 🔹 4종 이후: SkillManager의 combined 덱 로직 사용
            bool drawActive = sm.ShouldDrawActiveFromCombinedDeck();

            if (drawActive)
            {
                isActive = true;
                selectedActiveSkill = DrawActiveSkill();

                if (selectedActiveSkill == null)
                {
                    // 액티브가 다 막혔으면 패시브로
                    isActive = false;
                    selectedPassiveSkill = DrawPassiveSkill();
                }
            }
            else
            {
                isActive = false;
                selectedPassiveSkill = DrawPassiveSkill();

                if (selectedPassiveSkill == null)
                {
                    // 패시브도 못 뽑으면 마지막 희망으로 액티브 시도
                    isActive = true;
                    selectedActiveSkill = DrawActiveSkill();
                }
            }
        }
    }



    private ActiveSkillBase DrawActiveSkill()
    {
        const int maxTry = 30;

        ActiveSkillBase activeSkill = null;
        bool isDuplicateFromDeck = false;   // 덱 기준: 신규/중복 강화 여부

        for (int i = 0; i < maxTry; i++)
        {
            activeSkill = SkillManager.Instance.PreviewActiveSkillAutoFromDeck(out isDuplicateFromDeck);
            if (activeSkill == null)
                return null;    // 덱 자체에 후보가 없으면 그냥 실패

            // 이번 레벨업 세트에서 아직 안 나온 카드면 사용
            if (!usedActiveSkills.Contains(activeSkill))
            {
                break;          // ✅ 이 카드 채택
            }

            // 이미 세트에서 사용한 카드면 버리고 다시 시도
            activeSkill = null;
        }

        // maxTry 동안 전부 usedActiveSkills에 막혔으면 => 이번 카드는 액티브를 못 뽑음
        if (activeSkill == null)
            return null;        // 바깥 DrawSkillCard에서 패시브로 폴백

        // 여기서만 "이번 세트에서 사용한 액티브"로 등록
        usedActiveSkills.Add(activeSkill);

        selectedActiveSkill = activeSkill;
        isDuplicateActive = isDuplicateFromDeck;

        skillName.text = activeSkill.GetSkillName();

        // 중복 강화 카드면 +1성, 신규면 1성(또는 0성) 기획대로 표시
        if (isDuplicateActive)
            numOfStar = activeSkill.GetNumOfStar() + 1;
        else
            numOfStar = 1; // 처음 획득 카드는 1성으로 보여주고 싶으면 1, 0성부터면 0

        icon.sprite = activeSkill.GetIcon();
        description.text = activeSkill.GetAcquisitionDescriptionPlain();

        return activeSkill;
    }




    private PassiveSkillBase DrawPassiveSkill()
    {
        const int maxTry = 30;

        PassiveSkillBase passiveSkill = null;

        for (int i = 0; i < maxTry; i++)
        {
            passiveSkill = SkillManager.Instance.DrawPassiveSkillFromDeck();
            if (passiveSkill == null)
                return null;    // 덱에 후보 자체가 없으면 실패

            if (!usedPassiveSkills.Contains(passiveSkill))
            {
                break;          // ✅ 아직 안 나온 카드면 채택
            }

            passiveSkill = null;    // 이미 나온 카드면 버리고 다시 시도
        }

        if (passiveSkill == null)
            return null;    // 패시브 후보도 전부 막힌 경우

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
            SkillManager.Instance.CommitActiveSkillSelection(selectedActiveSkill);
        }
        else
        {
            Debug.Log(selectedPassiveSkill + " was selected");

            // ✅ 패시브 선택 확정 시 덱/카운트 반영
            SkillManager.Instance.CommitPassiveSkillSelection(selectedPassiveSkill);
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
