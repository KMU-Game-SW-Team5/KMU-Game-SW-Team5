using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Threading.Tasks;

public class InGameUIManager : MonoBehaviour
{
    public static InGameUIManager Instance { get; private set; }

    [Header("Player UI")]
    [SerializeField] PointBarUI playerHPUI;
    [SerializeField] PointBarUI playerEXPUI;
    [SerializeField] PlayerLVUI playerLVUI;
    [Header("Boss UI")]
    [SerializeField] GameObject bossStatusPanel;
    [SerializeField] PointBarUI bossHPUI;
    [SerializeField] TextMeshProUGUI bossNameUI;
    [Header("LevelUp UI")]
    [SerializeField] LevelUpUI levelUpUI;
    [SerializeField] GameObject levelUpPanel;
    [Header("Ending UI")]
    [SerializeField] EndingUI endingUI;
    [Header("EnterNicname UI")]
    [SerializeField] EnterNicknameUI enterNicknameUI;
    [Header("Leaderboard UI")]
    [SerializeField] LeaderboardUI leaderboardUI;
    [SerializeField] EntryUI playerEntryUI;
    [Header("Options UI")]
    [SerializeField] OptionsPanel optionsUI;
    [SerializeField] KeyCode optionToggleKey = KeyCode.Escape;
    [Header("FullMap UI")]
    [SerializeField] MinimapUI minimapUI;
    [SerializeField] GameObject fullMap;
    [SerializeField] KeyCode fullMapToggleKey = KeyCode.C;

    [Header("Etc")]
    [SerializeField] WaveTimerUI waveTimerUI;
    [HideInInspector] public List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();    // 동적 List
    [HideInInspector] public List<TextMeshProUGUI> skillKeysTexts = new List<TextMeshProUGUI>();  // 동적 List
    [HideInInspector] public List<TextMeshProUGUI> cooldownTexts = new List<TextMeshProUGUI>();  // 동적 List
    //[SerializeField] TimeManager timeManager;

    void Awake()
    {
        Instance = this;
        skillSlots = new List<SkillSlotUI>(GetComponentsInChildren<SkillSlotUI>());
        skillKeysTexts = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>());
        FilterSkillKeyTexts();
        cooldownTexts = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>(true));
        FilterCooldownTexts();
    }

    private void Start()
    {
        if (TimeManager.Instance == null)
        {
            Debug.LogWarning("[InGameUIManager] TimeManager.Instance is null in Start(). Skipping waveTimer subscription.");
            return;
        }

        if (waveTimerUI == null)
        {
            Debug.LogWarning("[InGameUIManager] waveTimerUI is not assigned in Start(). Skipping waveTimer subscription.");
            return;
        }

        waveTimerUI.SetRatio(TimeManager.Instance.DayRatio);
        TimeManager.Instance.OnCycleProgress += waveTimerUI.UpdateRotation;
        TimeManager.Instance.OnDayRatioChanged += waveTimerUI.SetRatio;
    }

    void OnDestroy()
    {
        try
        {
            if (TimeManager.Instance != null && waveTimerUI != null)
            {
                TimeManager.Instance.OnCycleProgress -= waveTimerUI.UpdateRotation;
                TimeManager.Instance.OnDayRatioChanged -= waveTimerUI.SetRatio;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[InGameUIManager] Exception during event unsubscription: {ex}");
        }

        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(optionToggleKey))
        {
            ToggleOptionsPanel();
        }

        // 전체 맵: 누르고 있는 동안만 보이도록 변경
        bool mapKeyHeld = Input.GetKey(fullMapToggleKey);
        ToggleFullMap(mapKeyHeld);
    }



    //void OnEnable()
    //{
    //    TimeManager.Instance.OnCycleProgress += waveTimerUI.UpdateRotation; 
    //    TimeManager.Instance.OnDayRatioChanged += waveTimerUI.SetRatio;
    //}
    //void OnDisable()
    //{
    //    TimeManager.Instance.OnCycleProgress -= waveTimerUI.UpdateRotation;
    //    TimeManager.Instance.OnDayRatioChanged -= waveTimerUI.SetRatio;
    //}

    // -----------------------------
    // About Player
    // -----------------------------
    public void UpdatePlayerHPUI(float newHP, float maxHP) { playerHPUI.SetPointUI(newHP, maxHP); }
    public void UpdatePlayerEXPUI(float newEXP, float maxEXP) { playerEXPUI.SetPointUI(newEXP, maxEXP); }
    public void UpdatePlayerLVUI(int newLevel) { playerLVUI.SetLV(newLevel); }

    // -----------------------------
    // About BossMonster
    // -----------------------------
    public void AppearBossUI(float newHP, float maxHP, string bossName = "BossMonster")
    {
        bossStatusPanel.SetActive(true);
        bossNameUI.text = bossName;
        UpdateBossHPUI(newHP, maxHP);
    }
    public void DisappearBossUI() { bossStatusPanel.SetActive(false); }
    public void UpdateBossHPUI(float newHP, float maxHP) { bossHPUI.SetPointUI(newHP, maxHP); }

    // -----------------------------
    // About Skill
    // -----------------------------
    public void UseSkill(int index, float cooldownTime) { skillSlots[index].ActivateCooldown(cooldownTime); }
    public void UpdateIcon(int index, Sprite newSkillSprite) { skillSlots[index].SetIcon(newSkillSprite); }

    // 스킬 키 텍스트 UI 업데이트 
    public void SetSkillKeys(KeyCode[] skillKeysArray)
    {

        for (int i = 0; i < skillKeysArray.Length; i++)
        {
            skillKeysTexts[i].text = skillKeysArray[i].ToString();  // 각 스킬에 대응하는 키 텍스트를 설정
        }
    }

    // 쿨타임 텍스트만 필터링하는 함수
    private void FilterCooldownTexts()
    {
        List<TextMeshProUGUI> cooldownList = new List<TextMeshProUGUI>();

        foreach (var text in cooldownTexts)
        {
            // 쿨타임 텍스트에만 특정 태그나 이름 등을 기준으로 필터링 가능
            if (text.gameObject.name.Contains("Cooldown"))  // 예시: 이름에 "Cooldown"이 포함된 경우
            {
                cooldownList.Add(text);
            }
        }

        cooldownTexts = cooldownList;  // 쿨타임 텍스트만 남기도록 List 갱신
    }

    // SkillKey 텍스트만 필터링하는 함수
    private void FilterSkillKeyTexts()
    {
        List<TextMeshProUGUI> skillKeyList = new List<TextMeshProUGUI>();

        // skillKeys 배열의 각 TextMeshProUGUI를 확인
        foreach (var text in skillKeysTexts)
        {
            // "SkillKey"라는 단어가 포함된 텍스트만 필터링
            if (text.gameObject.name.Contains("SkillKey")) // 이름에 "SkillKey"가 포함된 경우
            {
                skillKeyList.Add(text);
            }
        }

        // 필터링된 텍스트들만 List로 갱신
        skillKeysTexts = skillKeyList;
    }

    // -----------------------------
    // About LevelUp
    // -----------------------------
    public void ShowLevelUpUI()
    {
        levelUpPanel.SetActive(true);
        levelUpUI.ShowCards();        
    }

    // -----------------------------
    // About FullMap
    // -----------------------------
    // 기존 토글(다른 코드에서 사용될 수 있으니 유지)
    public void ToggleFullMap()
    {
        if (fullMap == null) return;
        fullMap.SetActive(!fullMap.activeSelf);
    }


    // 지도 펼치는 소리
    [SerializeField] private AudioClip mapOpenSfx;
    // SkillPanel.Toggle(bool)와 동일한 동작: show가 true일 때 열고, false일 때 닫음
    public void ToggleFullMap(bool show)
    {
        if (fullMap == null) return;

        bool isOpen = fullMap.activeSelf;

        if (show && !isOpen)
        {
            fullMap.SetActive(true);
            SFX_Manager.Instance.PlayOneShot(mapOpenSfx);
        }
        else if (!show && isOpen)
        {
            fullMap.SetActive(false);
            SFX_Manager.Instance.PlayOneShot(mapOpenSfx);
        }
    }

    public void UpdateCurrentRoom(Vector2Int gridPos)
    {
        foreach (var ui in MinimapUI.Instances)
        {
            ui.SetCurrentRoom(gridPos);
        }
    }
    public void UpdateRotation(float playerYaw)
    {
        foreach (var ui in MinimapUI.Instances)
        {
            ui.UpdateRotation(playerYaw);
        }
    }

    // -----------------------------
    // About EndingUI
    // -----------------------------
    public void ShowEndingUI(GameResult gameResult)
    {

        if (gameResult.IsClear) endingUI.SetupClear();
        else endingUI.SetupGameOver();

        endingUI.gameObject.SetActive(true);
        endingUI.SetValue(gameResult);
    }

    public void HideEndingUI()
    {
        endingUI.gameObject.SetActive(false);
    }

    // -----------------------------
    // About EnterNickNameUI
    // -----------------------------
    public void ShowEnterNicknamePanel()
    {
        enterNicknameUI.gameObject.SetActive(true);
    }

    public void HideEnterNicknamePanel()
    {
        enterNicknameUI.gameObject.SetActive(false);
    }

    // -----------------------------
    // About LeaderboardUI
    // -----------------------------
    public async Task ShowLeaderboardUI()
    {
        leaderboardUI.gameObject.SetActive(true);

        var entries = await LeaderboardService.Instance.LoadTopAsync();

        leaderboardUI.RefreshLeaderboard(entries);
        SetupPlayerEntry();
    }

    private void SetupPlayerEntry()
    {
        LeaderboardEntry myEntry = LeaderboardService.Instance.LastSubmittedEntry;
        playerEntryUI.SetData(myEntry, myEntry.Rank, true);
    }

    // -----------------------------
    // About OptionsUI
    // -----------------------------
    public void ToggleOptionsPanel()
    {
        optionsUI.gameObject.SetActive(!optionsUI.gameObject.activeSelf);
    }
}
