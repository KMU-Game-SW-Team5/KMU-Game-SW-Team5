using UnityEngine;

public class PlayerLevelUIBinder : MonoBehaviour
{
    [SerializeField] private PlayerLevelSystem levelSystem;
    [SerializeField] private InGameUIManager inGameUIManager;

    private void Awake()
    {
        if(inGameUIManager != null)
            inGameUIManager = GetComponent<InGameUIManager>();
    }

    private void OnEnable()
    {
        if (levelSystem == null || inGameUIManager == null)
        {
            Debug.LogWarning("PlayerLevelUIBinder: 참조가 비어 있습니다.");
            return;
        }

        levelSystem.OnLevelUp += HandleLevelUp;
        levelSystem.OnExpChanged += HandleExpChanged;
    }

    private void OnDisable()
    {
        if (levelSystem == null) return;

        levelSystem.OnLevelUp -= HandleLevelUp;
        levelSystem.OnExpChanged -= HandleExpChanged;
    }

    private void HandleExpChanged(float currentExp, float requiredExp)
    {
        // 경험치 바 UI 갱신
        inGameUIManager.UpdatePlayerEXPUI(currentExp, requiredExp);
    }

    private void HandleLevelUp(int level)
    {
        // 레벨 텍스트 UI 갱신
        inGameUIManager.UpdatePlayerLVUI(level);
    }
}
