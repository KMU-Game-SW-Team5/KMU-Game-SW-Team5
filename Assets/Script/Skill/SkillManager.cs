using OpenCover.Framework.Model;
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

    [Header("스킬 시전용 앵커 프리팹")]
    [Tooltip("스킬 타겟용 앵커 프리팹 (없으면 기본 빈 오브젝트 생성)")]
    [SerializeField] private GameObject skillAnchorPrefab;
    int mask;       // 레이가 무시할 레이어

    [Header("스킬 시전 위치 지정을 위한 변수")]
    [SerializeField] public static Camera cam;
    public static Vector3 forwardDirection;                      // 전방 방향을 가리키는 벡터
    [SerializeField] float maxSpellDistance = 1000f;      // 최대 시전 거리
    [SerializeField] float anchorLifetime = 10f;          // 앵커 오브젝트의 수명(최적화 변수)

    private void Start()
    {
        foreach (var skill in activeSkills)
        {
            if (skill != null)
                skill.Init();
        }

        // 스킬 개발 테스트가 종료되면 플레이어 스탯 변화시로 이동시킬 것
        UpdateSkillPower();

        // 카메라 연결
        cam = Camera.main;
        UpdateForwardDirection();

        // 레이가 무시할 레이어 설정
        mask = ~GameManager.Instance.GetIgnoreLayerMaskWithRay();
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
                    skill.TryUse(gameObject, CreateSkillAnchor()); // 플레이어 자신을 user로 전달, 조준한 곳의 첫 번째로 맞은 위치 전달
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


    // 바라보는 방향에 가장 먼저 맞은 곳에 프리팹을 생성해서 그 트랜스폼을 리턴함.
    // 스킬 시전할 때 위치를 지정할 때 사용됨.
    public Transform CreateSkillAnchor()
    {
        Vector3 origin = GetCameraPosition();
        UpdateForwardDirection();
        Vector3 direction = forwardDirection;

        GameObject anchorObj;
        Vector3 spawnPos;
        Transform targetTransform = null;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxSpellDistance, mask, QueryTriggerInteraction.Ignore))
        {
            spawnPos = hit.point;
            targetTransform = hit.transform;
        }
        else
        {
            spawnPos = origin + direction * maxSpellDistance;
        }

        if (skillAnchorPrefab != null)
            anchorObj = Instantiate(skillAnchorPrefab, spawnPos, Quaternion.identity);
        else
            anchorObj = new GameObject("SkillAnchor (Fallback)");


        // 🔹 Raycast로 맞은 오브젝트가 있다면 직접 부착 처리
        SkillAnchor anchor = anchorObj.GetComponent<SkillAnchor>();
        if (anchor != null && targetTransform != null)
            anchor.AttachTo(targetTransform, spawnPos);

        Destroy(anchorObj, anchorLifetime);
        return anchorObj.transform;
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
        ChangeProjectileAttributesForTest();
        //ChangeShotTypeForTest();
        //AnchorTest();

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
            //r.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            r.material.color = new Color(1f, 1f, 0f, 0.4f);
            Destroy(marker.GetComponent<Collider>());
        }
    }

    // 테스트용 투사체 개수 변화 (1, 2 : 투사체 가지 증가/감소, 3, 4 : 투사체 연속 발사 횟수 증가/감소)
    private void ChangeProjectileAttributesForTest()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    projectileSkill.IncreaseBranchCount();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    projectileSkill.DecreaseBranchCount();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {
                    projectileSkill.IncreaseBurstCount();
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            foreach (var skill in activeSkills)
            {
                if (skill is AS_ProjectType projectileSkill)
                {   
                    projectileSkill.DecreaseBurstCount();
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
                if (skill is AS_ProjectTypeLegacy projectileSkill)
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
                if (skill is AS_ProjectTypeLegacy projectileSkill)
                {
                    IShotType horizontalShot = new HorizontalMultiShot();
                    projectileSkill.SetShotType(horizontalShot);
                }
            }
        }
    }
}
