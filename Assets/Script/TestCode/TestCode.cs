using UnityEngine;

public class TestCode : MonoBehaviour
{
    [SerializeField] InGameUIManager inGameUIManager;
    int LV = 1;

    [Header("스킬 테스트 키")]
    [SerializeField] private KeyCode skill1Key = KeyCode.T;
    [SerializeField] private KeyCode skill2Key = KeyCode.Y;
    [SerializeField] private KeyCode skill3Key = KeyCode.U;
    [SerializeField] private KeyCode skill4Key = KeyCode.I;

    [Header("기타 테스트 키")]
    [SerializeField] private KeyCode expKey = KeyCode.G;  // 경험치 증가
    [SerializeField] private KeyCode hpKey = KeyCode.H;  // 체력 증가
    [SerializeField] private KeyCode bossAppearKey = KeyCode.J;  // 보스 등장
    [SerializeField] private KeyCode bossDamageKey = KeyCode.K;  // 보스 체력 깎기
    [SerializeField] private KeyCode bossDisappearKey = KeyCode.L;  // 보스 사라짐
    [SerializeField] private KeyCode levelUpKey = KeyCode.Colon;  // 레벨업
    [SerializeField] private KeyCode gameClearKey = KeyCode.C;  // 레벨업

    void OnEnable() { TimeManager.Instance.OnWaveChanged += ApplyWave; }
    void OnDisable() { TimeManager.Instance.OnWaveChanged -= ApplyWave; }

    void Update()
    {
        if (InputBlocker.IsInputBlocked)
            return;

        // 스킬 사용(쿨다운 시작)
        if (Input.GetKeyDown(skill1Key)) inGameUIManager.UseSkill(0, 5f);
        if (Input.GetKeyDown(skill2Key)) inGameUIManager.UseSkill(1, 5f);
        if (Input.GetKeyDown(skill3Key)) inGameUIManager.UseSkill(2, 5f);
        if (Input.GetKeyDown(skill4Key)) inGameUIManager.UseSkill(3, 5f);

        // 경험치 증가
        if (Input.GetKeyDown(expKey)) inGameUIManager.UpdatePlayerEXPUI(5, 40);

        // 체력 증가
        if (Input.GetKeyDown(hpKey)) inGameUIManager.UpdatePlayerHPUI(5, 100);

        // 보스 등장
        if (Input.GetKeyDown(bossAppearKey)) inGameUIManager.AppearBossUI(980, 980, "ABC");

        // 보스 체력 깎기
        if (Input.GetKeyDown(bossDamageKey)) inGameUIManager.UpdateBossHPUI(500, 980);

        // 보스 사라짐
        if (Input.GetKeyDown(bossDisappearKey)) inGameUIManager.DisappearBossUI();

        // 레벨업
        if (Input.GetKeyDown(levelUpKey)) inGameUIManager.UpdatePlayerLVUI(++LV);

        if (Input.GetKeyDown(gameClearKey)) GameManager.Instance.EndGame(true);
    }

    public void ApplyWave(bool isDay)
    {
        if (isDay) Debug.Log("Day");
        else Debug.Log("Night");
    }
}
