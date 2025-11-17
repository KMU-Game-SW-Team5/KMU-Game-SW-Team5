using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    [Header("Etc")]
    [HideInInspector] public List<SkillSlotUI> skillSlots = new List<SkillSlotUI>();    // 동적 List
    [HideInInspector] public List<TextMeshProUGUI> skillKeysTexts = new List<TextMeshProUGUI>();  // 동적 List
    [HideInInspector] public List<TextMeshProUGUI> cooldownTexts = new List<TextMeshProUGUI>();  // 동적 List
    [SerializeField] MinimapUI minimapUI;
    [SerializeField] WaveTimerUI waveTimerUI;
    [SerializeField] TimeManager timeManager;

    void Awake()
    {
        Instance = this;
        skillSlots = new List<SkillSlotUI>(GetComponentsInChildren<SkillSlotUI>());
        skillKeysTexts = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>());
        FilterSkillKeyTexts();
        cooldownTexts = new List<TextMeshProUGUI>(GetComponentsInChildren<TextMeshProUGUI>(true));
        FilterCooldownTexts();
    }

    void OnEnable() { timeManager?.AddProgressListener(waveTimerUI.UpdateRotation); }
    void OnDisable() { timeManager?.RemoveProgressListener(waveTimerUI.UpdateRotation); }

    // -----------------------------
    // About Player
    // -----------------------------
    public void UpdatePlayerHPUI(float newHP, float maxHP) { playerHPUI.SetPointUI(newHP, maxHP); }
    public void UpdatePlayerEXPUI(float newEXP, float maxEXP) { playerEXPUI.SetPointUI(newEXP, maxEXP); }
    public void UpdatePlayerLVUI(int newLevel) { playerLVUI.SetLV(newLevel); }

    // -----------------------------
    // About BossMonster
    // -----------------------------
    public void AppearBossUI(float maxHP, string bossName = "BossMonster")
    {
        bossStatusPanel.SetActive(true);
        bossNameUI.text = bossName;
        UpdateBossHPUI(maxHP, maxHP);
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
    // About Minimap
    // -----------------------------
    // public void UpdateMinimap(Sprite newMinimap) { minimapUI.SetMinimap(newMinimap); }
}
