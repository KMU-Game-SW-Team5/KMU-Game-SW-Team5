using System;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] SkillSlotUI[] skillSlots;
    [SerializeField] MinimapUI minimapUI;
    [SerializeField] WaveTimerUI waveTimerUI;
    [SerializeField] TimeManager timeManager;

    void Awake()
    {
        Instance = this;
    }

    void OnEnable() { timeManager.AddProgressListener(waveTimerUI.UpdateRotation); }
    void OnDisable() { timeManager.RemoveProgressListener(waveTimerUI.UpdateRotation); }

    // -----------------------------
    // About Player
    // -----------------------------
    public void UpdatePlayerHPUI(float newHP, float maxHP){ playerHPUI.SetPointUI(newHP, maxHP); }
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

    // -----------------------------
    // About Minimap
    // -----------------------------
    // public void UpdateMinimap(Sprite newMinimap) { minimapUI.SetMinimap(newMinimap); }
}
