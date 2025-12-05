using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillPanel : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SkillPanel Instance { get; private set; }

    [Header("실제 내용 패널 루트 (자식 패널)")]
    [SerializeField] private GameObject skillPanelWindow; // 실제로 보이는 패널(자식 오브젝트)

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

    // -----------------------------
    // 라이프사이클
    // -----------------------------
    private void Awake()
    {
        // 싱글톤 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 시작 시에는 창을 숨겨둔다.
        if (skillPanelWindow != null)
            skillPanelWindow.SetActive(false);
    }

    private void Update()
    {
        // 창이 열려있고 ScrollRect가 연결되어 있을 때만 스크롤 처리
        if (skillPanelWindow == null || !skillPanelWindow.activeInHierarchy)
            return;
        if (passiveScrollRect == null)
            return;

        float wheel = Input.mouseScrollDelta.y;   // 위로 스크롤: +, 아래로: -

        if (Mathf.Abs(wheel) > 0.0001f)
        {
            // ScrollRect.verticalNormalizedPosition: 1 = 맨 위, 0 = 맨 아래
            float pos = passiveScrollRect.verticalNormalizedPosition;
            pos += wheel * scrollSpeed;
            passiveScrollRect.verticalNormalizedPosition = Mathf.Clamp01(pos);
        }
    }

    // -----------------------------
    // 외부에서 호출할 인터페이스
    // -----------------------------

    /// <summary>
    /// 스킬 패널을 연다. (처음 열릴 때 한 번만 슬롯 생성)
    /// </summary>
    public void Show()
    {
        if (!initialized)
        {
            Init();             // 슬롯 생성
            initialized = true;
        }
        else
        {
            UpdateAllDescriptions();   // 기존 슬롯 설명/별만 갱신
        }

        if (skillPanelWindow != null)
            skillPanelWindow.SetActive(true);
    }

    /// <summary>
    /// 스킬 패널을 닫는다.
    /// </summary>
    public void Hide()
    {
        if (skillPanelWindow != null)
            skillPanelWindow.SetActive(false);
    }

    /// <summary>
    /// 열려 있으면 닫고, 닫혀 있으면 연다.
    /// 탭 버튼에 연결하기 좋은 토글 함수.
    /// </summary>
    public void Toggle(bool show)
    {
        if (skillPanelWindow == null) return;

        bool isOpen = skillPanelWindow.activeSelf;

        if (show && !isOpen)
        {
            Show();   // 닫혀 있는데 키를 눌렀을 때 → 한 번만 열기
            SFX_Manager.Instance.PlayOpen();
        }
        else if (!show && isOpen)
        {
            Hide();   // 열려 있는데 키를 뗐을 때 → 한 번만 닫기
            SFX_Manager.Instance.PlayClose();
        }
    }

    /// <summary>
    /// 현재 패널이 열려있는지 여부.
    /// </summary>
    public bool IsOpen =>
        skillPanelWindow != null && skillPanelWindow.activeSelf;

    // -----------------------------
    // 초기화 / 전체 갱신
    // -----------------------------
    private void Init()
    {
        RefreshAll();   // 슬롯 생성
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
                var slotComponent = slot.GetComponent<SkillUI_SkillPanel>();
                slotComponent.Setup(skill);
                slotComponent.UpdateDescription();
                activeSlots.Add(slotComponent);
            }
        }

        // 2) 패시브 스킬
        var passives = SkillManager.Instance.GetPassiveSkills();
        if (passives != null)
        {
            // 같은 패시브 여러 개 들고 있어도 UI에는 한 번만 표시
            var seen = new HashSet<PassiveSkillBase>();

            foreach (var skill in passives)
            {
                if (skill == null || seen.Contains(skill))
                    continue;

                seen.Add(skill);

                var slot = Instantiate(passiveSkillUIPrefab, passiveSkillContainer);
                var slotComponent = slot.GetComponent<SkillUI_SkillPanel>();
                slotComponent.Setup(skill);
                slotComponent.UpdateDescription();
                passiveSlots.Add(slotComponent);
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
                slot.UpdateDescription();
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
        var slotComponent = slot.GetComponent<SkillUI_SkillPanel>();
        slotComponent.Setup(newSkill);
        slotComponent.UpdateDescription();
        activeSlots.Add(slotComponent);
    }

    // 신규 획득이면 추가, 중복 획득이면 수정
    public void OnLearnPassiveSkill(PassiveSkillBase newSkill)
    {
        if (newSkill == null || passiveSkillUIPrefab == null || passiveSkillContainer == null)
            return;

        // 이미 같은 패시브를 표시 중인 슬롯이 있다면, 새로 만들지 않고 설명만 갱신
        var existing = passiveSlots.Find(slot =>
            slot != null && slot.IsSamePassive(newSkill));

        if (existing != null)
        {
            existing.UpdateDescription();
        }
        else
        {
            // 처음 배우는 패시브면 슬롯 생성
            var slot = Instantiate(passiveSkillUIPrefab, passiveSkillContainer);
            var slotComponent = slot.GetComponent<SkillUI_SkillPanel>();
            slotComponent.Setup(newSkill);
            slotComponent.UpdateDescription();
            passiveSlots.Add(slotComponent);
        }
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
