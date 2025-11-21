using OpenCover.Framework.Model;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using static UnityEngine.UI.GridLayoutGroup;


public class SkillManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SkillManager Instance { get; private set; }

    // 플레이어 오브젝트
    public GameObject owner { get; private set; }

    [Header("장착된 액티브 스킬 목록")]
    [SerializeField] private List<ActiveSkillBase> activeSkills = new List<ActiveSkillBase>();

    [Header("장착된 패시브 스킬 목록")]
    [SerializeField] private List<PassiveSkillBase> passiveSkills = new List<PassiveSkillBase>();

    // 현재 적용 중인 적중시 효과 목록
    private readonly List<IHitEffect> runtimeEffects = new();

    [Header("스킬 발동 키 설정")]
    [SerializeField] private KeyCode[] skillKeys = { KeyCode.Q, KeyCode.E, KeyCode.R, KeyCode.F };

    [Header("에임 오프셋")]
    [SerializeField] public Vector3 aimOffset;  // 스킬이 시전되는 위치의 공통 오프셋

    [Header("스탯")]
    [SerializeField] public float magicStat = 10f;  // 마력 스탯 
    public float GetMagicStat() { return magicStat; }

    [Header("스킬 시전용 앵커 프리팹")]
    [Tooltip("스킬 타겟용 앵커 프리팹 (없으면 기본 빈 오브젝트 생성)")]
    [SerializeField] private GameObject skillAnchorPrefab;
    int mask;       // 레이가 무시할 레이어

    [Header("스킬 시전 위치 지정을 위한 변수")]
    [SerializeField] public static Camera cam;
    public static Vector3 forwardDirection;                      // 전방 방향을 가리키는 벡터
    [SerializeField] float maxSpellDistance = 1000f;      // 최대 시전 거리
    [SerializeField] float anchorLifetime = 10f;          // 앵커 오브젝트의 수명(최적화 변수)

    [Header("스킬 UI")]
    List<SkillSlotUI> skillSlots;
    List<TextMeshProUGUI> cooldownTexts;  // 쿨다운 텍스트 배열

    private void Awake()
    {
        // 싱글톤 기본 코드
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        owner = this.gameObject;   // SkillManager는 플레이어에게 붙어있음
    }

    private void Start()
    {
        foreach (var skill in activeSkills)
        {
            if (skill != null)
                skill.Init();
        }
        TestMethodsInStart();
        // UI 연결
        skillSlots = InGameUIManager.Instance.skillSlots;
        cooldownTexts = InGameUIManager.Instance.cooldownTexts;
        // InGameUIManager에서 스킬 키 텍스트 배열 설정
        InGameUIManager.Instance.SetSkillKeys(skillKeys);

        // TODO : 스킬 개발 테스트가 종료되면 스킬 습득시에만 호출되게 할 것.
        for (int i = 0; i < activeSkills.Count; i++)
        {
            UpdateSkillIcon(i);
        }

        // 카메라 연결
        cam = Camera.main;
        UpdateForwardDirection();

        // 레이가 무시할 레이어 설정
        mask = ~GameManager.Instance.GetIgnoreLayerMaskWithRay();
    }


    private void Update()
    {
        // 테스트용 코드
        TestMethodsInUpdate();

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

    // 액티브 스킬 추가
    public void AddActiveSkill(ActiveSkillBase newSkill)
    {
        if (!activeSkills.Contains(newSkill))
            activeSkills.Add(newSkill);
    }
    // 액티브 스킬 제거
    public void RemoveAcvtiveSkill(ActiveSkillBase skill)
    {
        if (activeSkills.Contains(skill))
            activeSkills.Remove(skill);
    }

    // 패시브 스킬 추가
    public void AddPassiveSkill(PassiveSkillBase skill)
    {
        passiveSkills.Add(skill);
        if (skill is PS_AddHitEffectType addHitSkill)
        {
            foreach (var effSO in addHitSkill.hitEffects)
                runtimeEffects.Add(effSO.CreateEffectInstance());
        }
    }

    // 스킬 적중 발생시 호출
    public void OnHit(HitContext ctx)
    {
        foreach (var eff in runtimeEffects)
            if (eff.CanApply(ctx))
                eff.Apply(ctx);
    }

    // 스킬 습득시 호출
    public void OnSkillGetted()
    {
        
    }

    // 스킬들 쿨타임 감소
    public void UpdateSkillsCooldown()
    {
        for (int i = 0; i < activeSkills.Count; i++)
        {
            if (activeSkills[i] != null)
            {
                activeSkills[i].UpdateCooldown();
                UpdateSkillCoolDownUI(i);
            }
        }
    }

    // 스킬 쿨타임 UI 업데이트
    public void UpdateSkillCoolDownUI(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillSlots.Count)
        {
            float remainingCooldown = activeSkills[skillIndex].GetCooldown();

            // 쿨타임을 표시할 텍스트를 정수나 소수 첫째 자리로 처리
            string cooldownText = remainingCooldown >= 1f
                ? Mathf.Floor(remainingCooldown).ToString()  // 1 이상일 경우 정수로
                : remainingCooldown.ToString("F1");         // 1 이하일 경우 소수점 첫째자리로

            cooldownTexts[skillIndex].text = cooldownText;

            // 쿨타임이 끝났으면 비활성화
            if (remainingCooldown <= 0f)
            {
                cooldownTexts[skillIndex].gameObject.SetActive(false); // 비활성화
            }
            else
            {
                cooldownTexts[skillIndex].gameObject.SetActive(true);  // 활성화
            }

            // 시계방향 필터 적용
            if (skillIndex >= 0 && skillIndex < skillSlots.Count)
            {
                skillSlots[skillIndex].SetCooldownRatio(activeSkills[skillIndex].GetCooldownRatio());
            }
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

        if (Physics.Raycast(origin, direction, out RaycastHit hit,
                            maxSpellDistance, mask, QueryTriggerInteraction.Collide))
        {
            spawnPos = hit.point;
            targetTransform = hit.transform;

            // 몬스터를 맞췄다면 루트에 붙이도록 보정
            if (hit.collider.CompareTag("Monster") || hit.collider.CompareTag("Boss"))
            {
                targetTransform = hit.transform.root;
            }

            // 디버그용
            Debug.Log($"[SkillManager] Raycast hit: {hit.collider.name} (tag: {hit.collider.tag})");
        }
        else
        {
            spawnPos = origin + direction * maxSpellDistance;
            Debug.Log("[SkillManager] Raycast hit nothing. Anchor at max distance.");
        }

        if (skillAnchorPrefab != null)
            anchorObj = Instantiate(skillAnchorPrefab, spawnPos, Quaternion.identity);
        else
            anchorObj = new GameObject("SkillAnchor (Fallback)");

        SkillAnchor anchor = anchorObj.GetComponentInChildren<SkillAnchor>();
        if (anchor != null && targetTransform != null)
        {
            anchor.AttachTo(targetTransform, spawnPos);
        }

        Destroy(anchorObj, anchorLifetime);
        return anchorObj.transform;
    }



    // 스킬 아이콘 업데이트, 스킬 습득시 호출
    public void UpdateSkillIcon(int skillIndex)
    {
        if (skillIndex >= 0 && skillIndex < skillSlots.Count)
        {
            if (activeSkills[skillIndex].GetIcon() != null)
            {
                activeSkills[skillIndex].GetIcon().ToString();
                //skillSlots[skillIndex].SetIcon(activeSkills[skillIndex].GetIcon());
                var slot = skillSlots[skillIndex];
                var skill = activeSkills[skillIndex];
                var icon = skill.GetIcon();

                Debug.Log($"slot: {slot}, skill: {skill}, icon: {icon}");

                slot.SetIcon(icon);

            }
            else
            {
                Debug.Log("아이콘 발견 안됨.");
            }
        }
    }


    // ===============================================테스트 함수들======================================================
    // Start()에서 호출되어야 하는 테스트 함수들의 집합
    public void TestMethodsInStart()
    {
        LoadInitialPassiveSkills();
    }
    // Update()에서 호출되어야 하는 테스트 함수들의 집합
    public void TestMethodsInUpdate()
    {
        ChangeProjectileAttributesForTest();
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

    // ======================================================
    // 테스트용: 씬에 배치된 초기 패시브 스킬 자동 등록
    // ======================================================
    public void LoadInitialPassiveSkills()
    {
        runtimeEffects.Clear();   // 혹시나 중복을 막기 위해 초기화

        foreach (var skill in passiveSkills)
        {
            if (skill == null) continue;

            if (skill is PS_AddHitEffectType addHitSkill)
            {
                foreach (var effSO in addHitSkill.hitEffects)
                {
                    if (effSO != null)
                        runtimeEffects.Add(effSO.CreateEffectInstance());
                }
            }
        }

        Debug.Log($"[SkillManager] 초기 패시브 스킬 {passiveSkills.Count}개 적용됨, " +
                  $"런타임 효과 {runtimeEffects.Count}개 생성됨.");
    }



}
