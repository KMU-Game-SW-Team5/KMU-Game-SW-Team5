using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public bool IsPaused { get; private set; }

    [SerializeField] private LayerMask ignoreLayerMaskWithRay;  // 스킬 타겟팅에서 무시할 레이어

    // Ending을 위한 필드
    [SerializeField] private InGameUIManager inGameUIManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private PlayerLevelSystem playerLevelSystem;
    [SerializeField] private KillCounter killCounter;

    private void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);  // 씬 이동 시 파괴되지 않음

        InitializeGameSettings();
    }

    // 각종 전역 세팅 초기화
    private void InitializeGameSettings()
    {
        int projectile = LayerMask.NameToLayer("Projectile");
        int player = LayerMask.NameToLayer("Player");
        int skillAnchor = LayerMask.NameToLayer("SkillAnchor");

        // --- Projectile 관련 충돌 전부 무시 ---
        Physics.IgnoreLayerCollision(projectile, projectile);     // 투사체끼리
        Physics.IgnoreLayerCollision(player, projectile);         // 플레이어와 투사체
        Physics.IgnoreLayerCollision(projectile, skillAnchor);    // 투사체와 스킬 앵커

        // --- 스킬 타겟팅용 무시 레이어 ---
        ignoreLayerMaskWithRay = LayerMask.GetMask("Projectile", "SkillAnchor");
    }


    public LayerMask GetIgnoreLayerMaskWithRay()
    {
        return ignoreLayerMaskWithRay;
    }

    /// <summary>
    /// 게임 종료 시 호출 (매개변수는 bool값이며, true : Clear, false : Game Over)
    /// </summary>
    public void EndGame(bool isClear)
    {
        GameResult result = GetGameResult(isClear);

        inGameUIManager.ShowEndingUI(result);
    }

    public void CheckAndEndGame()
    {
        bool isEnd = false;

        int difficult = SettingsService.GameDifficulty;
        switch (difficult)
        {
            case 0:
                if (KillCounter.Instance.TotalBossKills == 1) isEnd = true;
                break;
            case 1:
                if (KillCounter.Instance.TotalBossKills == 2) isEnd = true;
                break;
            case 2:
                if (KillCounter.Instance.TotalBossKills == 3) isEnd = true;
                break;
        }

        if (isEnd) EndGame(true);
    }

    /// <summary>
    /// 게임 상태 요청 함수 (매개변수는 bool값이며, true : Clear, false : Game Over)
    /// </summary>
    public GameResult GetGameResult(bool isClear)
    {
        if (timeManager == null || playerLevelSystem == null || killCounter == null)
        {
            Debug.LogError("GameManager : 참조 null");
            return new GameResult();
        }

        return new GameResult(
            isClear,
            (float)timeManager.Elapsed,
            playerLevelSystem.Level,
            killCounter.TotalKills
        );
    }
}
