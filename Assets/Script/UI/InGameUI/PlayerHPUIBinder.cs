using UnityEngine;

public class PlayerHPUIBinder : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private InGameUIManager inGameUIManager;

    private void Awake()
    {
        if (inGameUIManager == null)
            inGameUIManager = GetComponent<InGameUIManager>();
    }

    private void OnEnable()
    {
        if (player == null || inGameUIManager == null)
        {
            Debug.LogWarning("PlayerHPUIBinder: 참조가 비어 있습니다.");
            return;
        }

        player.OnHPChanged += HandleHPChanged;
    }

    private void OnDisable()
    {
        if (player == null) return;

        player.OnHPChanged -= HandleHPChanged;
    }

    private void HandleHPChanged(int currentHP, int maxHP)
    {
        inGameUIManager.UpdatePlayerHPUI((float)currentHP, (float)maxHP);
    }
}
