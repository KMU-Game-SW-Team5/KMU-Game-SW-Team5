using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    [Header("장착된 액티브 스킬 목록")]
    [SerializeField] private List<ActiveSkillBase> activeSkills = new List<ActiveSkillBase>();

    [Header("스킬 발동 키 설정")]
    [SerializeField] private KeyCode[] skillKeys = { KeyCode.Q, KeyCode.E, KeyCode.R, KeyCode.F };

    [Header("에임 오프셋")]
    [SerializeField] public Vector3 aimOffset;  // 스킬이 시전되는 위치의 공통 오프셋

    [Header("스탯")]
    [SerializeField] public float magicStat = 10f;  // 마력 스탯 

    [Header("스킬 시전 위치 지정을 위한 변수")]
    [SerializeField] public static Camera cam;
    public static Vector3 forwardDirection;                      // 전방 방향을 가리키는 벡터
    [SerializeField] float maxSpellDistance = 1000f;      // 최대 시전 거리
    [SerializeField] float anchorLifetime = 10f;          // 앵커 오브젝트의 수명(최적화 변수)

    private void Start()
    {
        // 테스트용 코드임을 감안할 것.
        foreach (var skill in activeSkills)
        {
            // 발사형 스킬을 직선형 발사로 초기화함.
            if (skill is AS_ProjectType projectileSkill)    // 타입 체크 & 다운캐스팅
            {
                ForwardSingleShot linear = new ForwardSingleShot();
                projectileSkill.SetShotType(linear);
            }
        }



        // 스킬 개발 테스트가 종료되면 플레이어 스탯 변화시로 이동시킬 것
        UpdateSkillPower();

        // 카메라 연결
        cam = Camera.main;
        UpdateForwardDirection();
    }


    private void Update()
    {
        // 테스트용 코드
        Test();

        // 쿨타임 갱신
        UpdateSkillsCooldown();

        // 입력 처리 
        HandleSkillInput();
    }

    // 전방 방향 리턴하는 클래스 함수
    public static Vector3 GetForwardDirection()
    {
        UpdateForwardDirection();
        return forwardDirection;
    }
    // 캠 위치 리턴하는 클래스 함수
    public static Vector3 GetCameraPosition()
    {
        return cam.transform.position;
    }

    // 전방 방향 벡터 업데이트
    private static void UpdateForwardDirection()
    {
        forwardDirection = cam.transform.forward;
    }

    // 스킬 키 입력시 대응되는 스킬 사용 시도
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

    // 스킬들 쿨타임 감소
    public void UpdateSkillsCooldown()
    {
        foreach (var skill in activeSkills)
        {
            if (skill != null)
                skill.UpdateCooldown();
        }
    }


    // 바라보는 방향에 가장 먼저 맞은 곳에 오브젝트를 생성해서 그 트랜스폼을 리턴함.
    // 스킬 시전할 때 위치를 지정할 때 사용됨.
    public Transform CreateSkillAnchor()
    { 
        Vector3 origin = GetCameraPosition();    // 레이의 시작 벡터
        UpdateForwardDirection();
        Vector3 direction = forwardDirection;               // 레이의 방향

        // 테스트를 위해 레이 그림. 나중에 삭제해야 함.
        //Debug.DrawRay(origin, direction * maxSpellDistance, Color.red, 3f);

        GameObject anchor = new GameObject("SkillAnchor");  // 앵커 오브젝트 생성. 위치는 레이로 결정.
        // 앵커 오브젝트의 소멸은 각 스킬에서 관리하도록 함.

        // 레이 발사
        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxSpellDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            // 레이에 맞은 위치로 앵커 배치
            anchor.transform.position = hit.point;
        }
        else
        {
            // 맞지 않으면 설정한 최대 거리에 앵커 배치
            anchor.transform.position = origin + direction * maxSpellDistance;
        }
        return anchor.transform;
    }

    // 플레이어의 능력치 변화를 스킬들에 반영해줌.
    public void UpdateSkillPower()
    {
        foreach (var skill in activeSkills)
        {
            skill.magicStat = this.magicStat;
        }
    }


    // 테스트 코드의 집합
    public void Test()
    {
        ChangeProjectileNumForTest();
        ChangeShotTypeForTest();
        AnchorTest();

    }

    // 앵커가 제대로 생성되는지 확인하는 테스트(좌클릭 시 앵커 생성)
    private void AnchorTest()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Transform anchorT = CreateSkillAnchor();
            GameObject anchor = anchorT.gameObject;
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.SetParent(anchorT);
            marker.transform.localPosition = Vector3.zero;
            marker.transform.localScale = Vector3.one * 0.3f;
            Renderer r = marker.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.material.color = new Color(1f, 1f, 0f, 0.4f);
            Destroy(marker.GetComponent<Collider>());
        }
    }
    // 테스트용 투사체 개수 변화 (1 : 투사체 개수 증가, 2 : 투사체 개수 감소)
    private void ChangeProjectileNumForTest()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    projectileSkill.IncreaseProjectileNum();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    projectileSkill.DecreaseProjectileNum();
                }
            }
        }
    }

    // 테스트용 투사 방식 변경 (3 : 직선, 4 : 가로)
    public void ChangeShotTypeForTest()
    {
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    IShotType forwardShot = new ForwardSingleShot();
                    projectileSkill.SetShotType(forwardShot);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    IShotType horizontalShot = new HorizontalMultiShot();
                    projectileSkill.SetShotType(horizontalShot);
                }
            }
        }
    }
}
