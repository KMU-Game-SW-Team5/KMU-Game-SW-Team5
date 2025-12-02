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
            // 🔹 액티브는 기존 로직 그대로
            string template = activeSkillRef.GetDescriptionTemplate();  // "…{damage}…"
            int dmg = activeSkillRef.GetDamageInt();
            string dmgStr = dmg.ToString();

            string hex = ColorUtility.ToHtmlStringRGB(damageHighlightColor);
            string colored = $"<color=#{hex}>{dmgStr}</color>";

            descText.text = template.Replace("{damage}", colored);

            int currentStar = activeSkillRef.GetNumOfStar();
            if (currentStar != numOfStars)
            {
                SetStarCount(currentStar);
            }
        }
        else if (!isActiveSkill && passiveSkillRef != null)
        {
            // 🔹 패시브는 "획득 횟수 + 누적 수치"를 강조색으로 표시
            int count = 1;

            if (SkillManager.Instance != null)
            {
                int acquired = SkillManager.Instance.GetPassiveAcquireCount(passiveSkillRef);
                if (acquired > 0)
                    count = acquired;
            }

            float per = passiveSkillRef.ValuePerStack;
            string result;

            // 강조색 코드 만들기 (액티브랑 같은 색)
            string hex = ColorUtility.ToHtmlStringRGB(damageHighlightColor);

            if (Mathf.Approximately(per, 0f))
            {
                // 수치 정보가 없으면: 기본 설명 + (x{count})에 count만 강조
                string baseDesc = passiveSkillRef.GetSkillDescription();
                string coloredCount = $"<color=#{hex}>{count}</color>";
                result = $"{baseDesc} (x{coloredCount})";
            }
            else
            {
                // 수치 정보가 있으면: 템플릿 기반으로 value / count 둘 다 강조
                float total = per * count;

                string template = passiveSkillRef.StackedDescriptionTemplate;

                string coloredValue = $"<color=#{hex}>{total}</color>";
                string coloredCount = $"<color=#{hex}>{count}</color>";

                result = template
                    .Replace("{value}", coloredValue)
                    .Replace("{count}", coloredCount);
            }

            descText.text = result;
        }

    }

    // 획득시 중복 체크에 쓰이는 판별 함수
    public bool IsSamePassive(PassiveSkillBase skill)
    {
        return !isActiveSkill && passiveSkillRef == skill;
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
