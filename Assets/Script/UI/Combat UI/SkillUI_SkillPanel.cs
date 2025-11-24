using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI_SkillPanel : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descText;

    [Header("액티브 스킬의 경우에만 넣음")]
    [SerializeField] private GameObject starContainer;   // 별들을 담을 부모
    [SerializeField] private GameObject starGO;          // 별 Prefab (Image 하나짜리)

    [Header("강조 색 설정")]
    [SerializeField] private Color damageHighlightColor = new Color(1f, 0.8f, 0.2f);

    private int numOfStars;
    private bool isActiveSkill;

    private ActiveSkillBase activeSkillRef;
    private PassiveSkillBase passiveSkillRef;

    #region Setup

    // 액티브 스킬을 표시하는 경우
    public void Setup(ActiveSkillBase activeSkill)
    {
        isActiveSkill = true;
        activeSkillRef = activeSkill;
        passiveSkillRef = null;

        if (iconImage != null)
            iconImage.sprite = activeSkill.GetIcon();
        if (nameText != null)
            nameText.text = activeSkill.GetSkillName();

        // ⭐ 성급 UI 초기화
        if (starContainer != null)
            starContainer.SetActive(true);

        numOfStars = activeSkill.GetNumOfStar();
        RebuildStars();
    }

    // 패시브 스킬을 표시하는 경우
    public void Setup(PassiveSkillBase passiveSkill)
    {
        isActiveSkill = false;
        passiveSkillRef = passiveSkill;
        activeSkillRef = null;

        if (iconImage != null)
            iconImage.sprite = passiveSkill.GetIcon();
        if (nameText != null)
            nameText.text = passiveSkill.GetSkillName();

        // 패시브는 별 UI 사용 X (원하면 여기서 0성으로 표시해도 됨)
        if (starContainer != null)
            starContainer.SetActive(false);

        numOfStars = 0;
        ClearStars();
    }

    #endregion

    // 설명과 별 개수 업데이트
    public void UpdateDescription()
    {
        if (descText == null) return;

        if (isActiveSkill && activeSkillRef != null)
        {
            // 🔹 설명 갱신 (데미지 숫자에만 색 입히기)
            string template = activeSkillRef.GetDescriptionTemplate();  // "…{damage}…"
            int dmg = activeSkillRef.GetDamageInt();
            string dmgStr = dmg.ToString();

            string hex = ColorUtility.ToHtmlStringRGB(damageHighlightColor);
            string colored = $"<color=#{hex}>{dmgStr}</color>";

            descText.text = template.Replace("{damage}", colored);

            // 🔹 성급이 바뀌었을 수 있으니 스킬 데이터에서 다시 읽어서 UI 갱신
            int currentStar = activeSkillRef.GetNumOfStar();
            if (currentStar != numOfStars)
            {
                SetStarCount(currentStar);
            }
        }
        else if (!isActiveSkill && passiveSkillRef != null)
        {
            // 패시브는 기존 방식대로
            descText.text = passiveSkillRef.GetSkillDescription();
        }
    }

    // 스킬 획득 팝업 등에 쓰고 싶으면 이런 것도 가능:
    public string GetAcquisitionDescriptionForPopup()
    {
        if (isActiveSkill && activeSkillRef != null)
        {
            string template = activeSkillRef.GetDescriptionTemplate();
            string formula = activeSkillRef.GetDamageFormulaString(); // "120 + (120% 마력)"

            string hex = ColorUtility.ToHtmlStringRGB(damageHighlightColor);
            string coloredFormula = $"<color=#{hex}>{formula}</color>";

            return template.Replace("{damage}", coloredFormula);
        }

        return string.Empty;
    }

    // (선택) 외부에서 UI용 성급을 강제로 갱신하고 싶을 때
    public void SetStarCount(int starCount)
    {
        numOfStars = Mathf.Max(0, starCount);
        RebuildStars();
    }

    // (선택) 1성 업그레이드 시, UI만 한 단계 올릴 때 사용 가능
    // 실제 데이터(activeSkillRef)의 성급도 같이 변경해야 일관됨
    public void IncreaseStar()
    {
        if (!isActiveSkill) return;

        numOfStars++;
        RebuildStars();
    }

    #region Star Instantiation

    private void ClearStars()
    {
        if (starContainer == null) return;

        Transform parent = starContainer.transform;
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }

    private void RebuildStars()
    {
        if (starContainer == null || starGO == null)
            return;

        ClearStars();

        Transform parent = starContainer.transform;
        for (int i = 0; i < numOfStars; i++)
        {
            Instantiate(starGO, parent);
        }
    }

    #endregion
}
