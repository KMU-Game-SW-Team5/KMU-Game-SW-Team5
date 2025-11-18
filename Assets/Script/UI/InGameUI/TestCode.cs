using UnityEngine;

public class TestCode : MonoBehaviour
{
    [SerializeField] InGameUIManager inGameUIManager;
    //[SerializeField] TimeManager timeManager;
    int LV = 1;

    void OnEnable() { TimeManager.Instance.OnWaveChanged += ApplyWave; }
    void OnDisable() { TimeManager.Instance.OnWaveChanged -= ApplyWave; }

    void Update()
    {
        if (InputBlocker.IsInputBlocked)
            return;

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

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SkillData skillData1 = new SkillData();
            skillData1.skillName = "FireBall";
            skillData1.level = 5;
            skillData1.description = $"적에게 화염구 {skillData1.level}개를 발사하여 피해를 입힙니다.";

            SkillData skillData2 = new SkillData();
            skillData2.skillName = "IceSpear";
            skillData2.level = 1;
            skillData2.description = $"적에게 얼음으로 이루어진 창 {skillData2.level}개를 발사하여 피해를 입힙니다.";

            SkillData skillData3 = new SkillData();
            skillData3.skillName = "WaterBall";
            skillData3.level = 2;
            skillData3.description = $"적에게 물로 이루어진 벽을 {skillData3.level}개를 발사하여 피해를 입히고 밀어냅니다.";

            SkillData[] options =
            {
                skillData1, skillData2, skillData3
            };
            inGameUIManager.ShowLevelUpUI(options);
        }
    }

    public void ApplyWave(bool isDay)
    {
        if (isDay) { Debug.Log("Day"); }
        else { Debug.Log("Night"); }
    }
}
