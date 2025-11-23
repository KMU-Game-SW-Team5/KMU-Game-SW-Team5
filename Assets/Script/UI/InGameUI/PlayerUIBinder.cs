using UnityEngine;

public class PlayerUIBinder : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerLevelSystem levelSystem;

    private void OnEnable()
    {
        if (player != null)
            player.OnHPChanged += HandleHPChanged;

        if (levelSystem != null)
        {
            levelSystem.OnLevelUp += HandleLevelUp;
            levelSystem.OnExpChanged += HandleExpChanged;            
        }
    }

    private void OnDisable()
    {
        if (player != null)
            player.OnHPChanged -= HandleHPChanged;

        if (levelSystem != null)
        {
            levelSystem.OnLevelUp -= HandleLevelUp;
            levelSystem.OnExpChanged -= HandleExpChanged;            
        }
    }

    private void HandleHPChanged(int currentHP, int maxHP)
    {
        InGameUIManager.Instance.UpdatePlayerHPUI((float)currentHP, (float)maxHP);
    }

    private void HandleExpChanged(float currentExp, float requiredExp)
    {
        // 경험치 바 UI 갱신
        InGameUIManager.Instance.UpdatePlayerEXPUI(currentExp, requiredExp);
    }

    private void HandleLevelUp(int level)
    {
        // 레벨 텍스트 UI 갱신
        InGameUIManager.Instance.UpdatePlayerLVUI(level);
    }
}
