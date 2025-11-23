using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanel : MonoBehaviour
{
    [Header("UI 컨테이너")]
    [SerializeField] private Transform activeSkillContainer;
    [SerializeField] private Transform passiveSkillContainer;

    [Header("UI 프리팹")]
    [SerializeField] private GameObject activeSkillUIPrefab;   // 액티브용 슬롯 프리팹
    [SerializeField] private GameObject passiveSkillUIPrefab;  // 패시브용 슬롯 프리팹

    [Header("스크롤 설정")]
    [SerializeField] private ScrollRect passiveScrollRect;     // 패시브 스킬 리스트용 ScrollRect
    [SerializeField] private float scrollSpeed = 0.15f;        // 휠 감도

    // 생성된 슬롯들 캐싱 (설명/별 갱신용)
    private readonly List<SkillUI_SkillPanel> activeSlots = new();
    private readonly List<SkillUI_SkillPanel> passiveSlots = new();

    private bool initialized = false;

    private void Start()
    {
        // 씬 진입 시 한 번만 전체 슬롯 생성
        RefreshAll();
        initialized = true;
    }

    // 패시브 스킬 스크롤 가능하게 함.
    private void Update()
    {
        // 패널이 켜져 있고 ScrollRect가 연결되어 있을 때만
        if (!gameObject.activeInHierarchy) return;
        if (passiveScrollRect == null) return;

        // 마우스 휠 입력 (UnityEngine.Input)
        float wheel = Input.mouseScrollDelta.y;   // 위로 스크롤: +, 아래로: -

        if (Mathf.Abs(wheel) > 0.0001f)
        {
            // ScrollRect.verticalNormalizedPosition:
            // 1 = 맨 위, 0 = 맨 아래
            float pos = passiveScrollRect.verticalNormalizedPosition;

            // 휠 방향에 따라 위/아래로 이동
            pos += wheel * scrollSpeed;

            passiveScrollRect.verticalNormalizedPosition = Mathf.Clamp01(pos);
        }
    }


    private void OnEnable()
    {
        // 패널이 다시 켜질 때는 기존 슬롯을 재사용하고, 설명/별만 갱신
        if (initialized)
        {
            UpdateAllDescriptions();
        }
        else
        {
            // 혹시 Start 전에 켜질 수 있으면 안전망
            RefreshAll();
            initialized = true;
        }
    }
     
    /// <summary>
    /// 전체 스킬 목록을 다시 그린다. (슬롯 싹 지우고 다시 생성)
    /// 새 스킬 세트가 크게 바뀐 경우 / 처음 초기화할 때 사용.
    /// </summary>
    public void RefreshAll()
    {
        ClearAllSlots();

        // 1) 액티브 스킬
        var actives = SkillManager.Instance.GetActiveSkills();
        if (actives != null)
        {
            foreach (var skill in actives)
            {
                var slot = Instantiate(activeSkillUIPrefab, activeSkillContainer);
                SkillUI_SkillPanel slot_component = slot.GetComponent<SkillUI_SkillPanel>();
                slot_component.Setup(skill);          // 액티브 스킬 참조 넘김
                slot_component.UpdateDescription();   // 현재 스탯 기반으로 설명/별 갱신 (이 안에서 처리)
                activeSlots.Add(slot_component);
            }
        }

        // 2) 패시브 스킬
        var passives = SkillManager.Instance.GetPassiveSkills();
        if (passives != null)
        {
            foreach (var skill in passives)
            {
                var slot = Instantiate(passiveSkillUIPrefab, passiveSkillContainer);
                SkillUI_SkillPanel slot_component = slot.GetComponent<SkillUI_SkillPanel>();
                slot_component.Setup(skill);          // 패시브 스킬 참조 넘김
                slot_component.UpdateDescription();   // 설명/별 갱신
                passiveSlots.Add(slot_component);
            }
        }
    }

    /// <summary>
    /// 스킬 설명 + 별 개수만 전부 업데이트 (스탯 변동 / 강화 후 호출).
    /// 슬롯은 다시 만들지 않는다.
    /// </summary>
    public void UpdateAllDescriptions()
    {
        foreach (var slot in activeSlots)
        {
            if (slot != null)
                slot.UpdateDescription();   // 여기서 설명 + 별 갱신까지 담당
        }

        foreach (var slot in passiveSlots)
        {
            if (slot != null)
                slot.UpdateDescription();
        }
    }

    /// <summary>
    /// 액티브 스킬을 새로 배웠을 때 UI에 즉시 슬롯 추가.
    /// </summary>
    public void OnLearnActiveSkill(ActiveSkillBase newSkill)
    {
        if (newSkill == null || activeSkillUIPrefab == null || activeSkillContainer == null)
            return;

        var slot = Instantiate(activeSkillUIPrefab, activeSkillContainer);
        SkillUI_SkillPanel slot_component = slot.GetComponent<SkillUI_SkillPanel>();
        slot_component.Setup(newSkill);          // 액티브 스킬 참조 넘김
        slot_component.UpdateDescription();   // 현재 스탯 기반으로 설명/별 갱신 (이 안에서 처리)
        activeSlots.Add(slot_component);
    }

    /// <summary>
    /// 패시브 스킬을 새로 배웠을 때 UI에 즉시 슬롯 추가.
    /// </summary>
    public void OnLearnPassiveSkill(PassiveSkillBase newSkill)
    {
        if (newSkill == null || passiveSkillUIPrefab == null || passiveSkillContainer == null)
            return;

        var slot = Instantiate(passiveSkillUIPrefab, passiveSkillContainer);
        SkillUI_SkillPanel slot_component = slot.GetComponent<SkillUI_SkillPanel>();
        slot_component.Setup(newSkill);          // 패시브 스킬 참조 넘김
        slot_component.UpdateDescription();   // 설명/별 갱신
        passiveSlots.Add(slot_component);
    }

    // -----------------------------
    // 내부 헬퍼
    // -----------------------------
    private void ClearAllSlots()
    {
        foreach (var slot in activeSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        activeSlots.Clear();

        foreach (var slot in passiveSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        passiveSlots.Clear();
    }
}
