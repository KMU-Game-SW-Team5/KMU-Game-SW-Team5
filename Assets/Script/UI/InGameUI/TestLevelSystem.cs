using UnityEngine;

public class TestLevelSystem : MonoBehaviour
{
    [SerializeField] private PlayerLevelSystem levelSystem;
    [SerializeField] private InGameUIManager inGameUIManager;

    private void OnEnable()
    {
        levelSystem.OnLevelUp += HandleLevelUp;
    }

    private void OnDisable()
    {
        levelSystem.OnLevelUp -= HandleLevelUp;
    }

    private void Update()
    {
        if (InputBlocker.IsInputBlocked)
            return;

        if (Input.GetKeyDown(KeyCode.U)) levelSystem.AddExp(300f);
    }

    private void HandleLevelUp(int newLevel)
    {
        SkillData[] options = GenerateOptions(newLevel);

        // 레벨업 UI 띄우기
        inGameUIManager.ShowLevelUpUI(options);
    }

    private SkillData[] GenerateOptions(int newLevel)
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

        return options;
    }
}
