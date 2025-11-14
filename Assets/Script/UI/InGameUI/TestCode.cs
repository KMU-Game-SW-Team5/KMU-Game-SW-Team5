using UnityEngine;

public class TestCode : MonoBehaviour
{
    [SerializeField] InGameUIManager inGameUIManager;
    [SerializeField] TimeManager timeManager;
    int LV = 1;

    void OnEnable() { timeManager.OnWaveChanged += ApplyWave; }
    void OnDisable() { timeManager.OnWaveChanged -= ApplyWave; }

    void Update()
    {
        // 1~4: 스킬 사용(쿨다운 시작)
        if (Input.GetKeyDown(KeyCode.Alpha1)) inGameUIManager.UseSkill(0, 5f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inGameUIManager.UseSkill(1, 5f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inGameUIManager.UseSkill(2, 5f);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inGameUIManager.UseSkill(3, 5f);

        // q: 경험치 증가
        if (Input.GetKeyDown(KeyCode.Q)) inGameUIManager.UpdatePlayerEXPUI(5, 40);

        // w: 체력 증가
        if (Input.GetKeyDown(KeyCode.W)) inGameUIManager.UpdatePlayerHPUI(5,100);

        // e: 보스 등장
        if (Input.GetKeyDown(KeyCode.E)) inGameUIManager.AppearBossUI(980, "ABC");

        // r: 보스 사라짐
        if (Input.GetKeyDown(KeyCode.R)) inGameUIManager.UpdateBossHPUI(500, 980);

        // t: 보스 체력 깎기
        if (Input.GetKeyDown(KeyCode.T)) inGameUIManager.DisappearBossUI();

        // y: 레벨업
        if (Input.GetKeyDown(KeyCode.Y)) inGameUIManager.UpdatePlayerLVUI(++LV);
    }

    public void ApplyWave(bool isDay)
    {
        if (isDay) { Debug.Log("Day"); }
        else { Debug.Log("Night"); }
    }
}
