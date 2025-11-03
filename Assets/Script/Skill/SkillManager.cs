using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("장착된 액티브 스킬 목록")]
    [SerializeField] private List<ActiveSkillBase> activeSkills = new List<ActiveSkillBase>();

    [Header("스킬 발동 키 설정")]
    [SerializeField] private KeyCode[] skillKeys = { KeyCode.Q, KeyCode.E, KeyCode.R, KeyCode.F };

    private void Start()
    {
        // 테스트용 코드임을 감안할 것.
        foreach (var skill in activeSkills)
        {
            // 발사형 스킬을 직선형 발사로 초기화함.
            if (skill is AS_ProjectType projectileSkill)    // 타입 체크 & 다운캐스팅
            {
                LinearShot linear = new LinearShot();
                projectileSkill.SetShotType(linear);
            }
        }
    }


    private void Update()
    {
        // 테스트용 코드
        ChangeShotTypeForTest();


        // 쿨타임 갱신
        foreach (var skill in activeSkills)
        {
            if (skill != null)
                skill.UpdateCooldown();
        }

        // 입력 처리 
        HandleSkillInput();
    }

    private void HandleSkillInput()
    {
        for (int i = 0; i < activeSkills.Count && i < skillKeys.Length; i++)
        {
            if (Input.GetKeyDown(skillKeys[i]))
            {
                ActiveSkillBase skill = activeSkills[i];
                if (skill != null)
                {
                    skill.TryUse(gameObject); // 플레이어 자신을 user로 전달
                }
            }
        }
    }

    // 스킬 추가
    public void AddSkill(ActiveSkillBase newSkill)
    {
        if (!activeSkills.Contains(newSkill))
            activeSkills.Add(newSkill);
    }
    // 스킬 제거
    public void RemoveSkill(ActiveSkillBase skill)
    {
        if (activeSkills.Contains(skill))
            activeSkills.Remove(skill);
    }

    // 테스트용 발사 방식 변경 (1 : 직선, 2 : 가로)
    public void ChangeShotTypeForTest()
    {
        // 1 입력시 직선으로 여러개 발사하는 방식으로 변경
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    LinearShot linear = new LinearShot();
                    projectileSkill.SetShotType(linear);
                    Debug.Log($"[Change] {projectileSkill.name} → SpreadShot으로 변경됨");
                }
            }
        }
        // 2 입력시 가로로 여러개 발사하는 방식으로 변경
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    HorizontalMultiShot spread = new HorizontalMultiShot
                    {
                        projectileCount = 5,   // 발사 개수
                        spreadAngle = 30f      // 퍼지는 각도
                    };

                    projectileSkill.SetShotType(spread);
                    Debug.Log($"[Change] {projectileSkill.name} → SpreadShot으로 변경됨 " +
                        $"(개수 {spread.projectileCount}, 각도 {spread.spreadAngle})");
                }
            }
        }
    }
}
