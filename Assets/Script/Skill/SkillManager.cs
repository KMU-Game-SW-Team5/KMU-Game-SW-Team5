using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;

public class SkillManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static SkillManager Instance { get; private set; }

    [SerializeField] private InputManager inputManager;

    // 플레이어 오브젝트
    public GameObject owner { get; private set; }
    public Player player;
    private PlayerAnimation playerAnimation;

    [Header("버프 적용해주는 스크립트")]
    [SerializeField] public BuffApplier buffApplier;

    [Header("스킬 시전 위치 (플레이어 자식 오브젝트들)")]
    [SerializeField] private Transform shotPos_Staff;
    [SerializeField] private Transform shotPos_LeftDown;
    [SerializeField] private Transform shotPos_Left;
    [SerializeField] private Transform shotPos_LeftUp;
    [SerializeField] private Transform shotPos_Up;
    [SerializeField] private Transform shotPos_RightUp;
    [SerializeField] private Transform shotPos_Right;
    [SerializeField] private Transform shotPos_RightDown;

    [Header("스킬 시전 위치 조절")]
    [SerializeField] public float frontDistance = 2f;
    [SerializeField] public float radius = 1f;



    // 현재 적용 중인 적중시 효과 목록
    private readonly List<IHitEffect> runtimeEffects = new();

    [Header("스킬 발동 키 설정")]
    [SerializeField] private KeyCode[] skillKeys = { KeyCode.Q, KeyCode.E, KeyCode.R, KeyCode.F };

    [Header("기본 공격")]
    [SerializeField] private ActiveSkillBase basicAttackSkill; // 기본 공격에 사용할 액티브 스킬
    [SerializeField, Tooltip("초당 몇 번까지 기본 공격 가능한지")]
    private float attackSpeed = 1f;
    public float GetAttackSpeed() => attackSpeed;
    public void AddAttackSpeed(float value) { attackSpeed += value; }

    // 내부용: 마지막 기본 공격 시각
    private float lastBasicAttackTime = -999f;

    // rate → 쿨타임(초) 변환
    private float BasicAttackCooldown
    {
        get
        {
            if (attackSpeed <= 0f) return 99999f;
            return 1f / attackSpeed;
        }
    }

    private float magicStat = 100f;  // 마력 스탯 

    public void SetMagicStat(int value) { magicStat = value; }

    public float GetMagicStat() { return magicStat; }
    public void AddMagicStat(float value) { magicStat += value; }

    public void AddMagicStatPercent(float percent) { magicStat *= 1 + percent; }

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

    [Header("사운드")]
    [SerializeField] private AudioSource skillAudioSource;

    // 시전 중에는 스킬 입력을 막기 위한 플래그
    private bool isCasting = false;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
        // 싱글톤 기본 코드
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        owner = this.gameObject;   // SkillManager는 플레이어에게 붙어있음
        player = GetComponent<Player>();
        InitalizeActiveSkills();
        if (skillAudioSource == null) skillAudioSource = GetComponent<AudioSource>();
        if (inputManager == null) inputManager = GetComponent<InputManager>();
    }

    private void Start()
    {
        playerAnimation = GetComponent<PlayerAnimation>();

        // 기본 공격 초기화
        basicAttackSkill.Initialize();

        // 액티브 스킬 쿨타임 초기화
        foreach (var skill in activeSkills)
        {
            if (skill != null)
                skill.InitializeCooldown();
        }

        // 덱 초기화
        if (allActiveDeck != null)
            allActiveDeck.ResetRuntimeFromInitial();  // 전체 액티브 덱: 초기 카드 풀 기준

        if (ownedActiveDeck != null)
            ownedActiveDeck.ClearRuntime();           // 획득 덱: 시작 시 비워두기

        if (passiveSkillDeck != null)
            passiveSkillDeck.ResetDeck();

        TestMethodsInStart();

        // UI 연결
        skillSlots = InGameUIManager.Instance.skillSlots;
        cooldownTexts = InGameUIManager.Instance.cooldownTexts;
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

        // 게임 끝나면 스킬 사용 불가
        if (!inputManager.GetMovable()) return;

        // 입력 처리 
        HandleBasicAttackInput();
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
        if (cam == null) cam = Camera.main;
        return cam.transform.position;
    }

    // 전방 방향 벡터 업데이트
    private static void UpdateForwardDirection()
    {
        forwardDirection = cam.transform.forward;
    }

    // 스킬 시전 위치 Transform 리턴
    public Transform GetCastTransform(ShotPositions shotPosition)
    {
        switch (shotPosition)
        {
            case ShotPositions.Staff:
                return shotPos_Staff;
            case ShotPositions.LeftDown:
                return shotPos_LeftDown;
            case ShotPositions.Left:
                return shotPos_Left;
            case ShotPositions.LeftUp:
                return shotPos_LeftUp;
            case ShotPositions.Up:
                return shotPos_Up;
            case ShotPositions.RightUp:
                return shotPos_RightUp;
            case ShotPositions.Right:
                return shotPos_Right;
            case ShotPositions.RightDown:
                return shotPos_RightDown;
            default:
                return shotPos_Staff;
        }
    }

    // 기본 공격 입력 처리 (좌클릭)
    private void HandleBasicAttackInput()
    {
        if (isCasting) return;
        if (basicAttackSkill == null) return;
        if (attackSpeed <= 0f) return;

        if (!Input.GetMouseButton(0))
            return;

        if (Time.time < lastBasicAttackTime + BasicAttackCooldown)
            return;

        bool executed = basicAttackSkill.TryUse(gameObject, CreateSkillAnchor(), true);


        if (executed)
        {
            lastBasicAttackTime = Time.time;

            AnimationType animType = basicAttackSkill.GetSkillAnimation();

            // 사운드 재생
            if (basicAttackSkill.castClip != null)
            {
                skillAudioSource.PlayOneShot(basicAttackSkill.castClip, 0.5f);
            }
            else
            {
                Debug.Log("cast clip is null");
            }


            if (animType == AnimationType.Straight)
            {
                float castTime = basicAttackSkill.GetCastTime();
                playerAnimation.PlayStraightFor(castTime);
            }
            else
            {
                playerAnimation.SetAnimation(animType);
            }

            float lockDuration = basicAttackSkill.GetPrepareTime() + basicAttackSkill.GetCastTime();
            if (lockDuration > 0f)
                StartCoroutine(LockSkillInputCoroutine(lockDuration));
        }
    }

    // 스킬 키 입력시 대응되는 스킬 사용 시도
    private void HandleSkillInput()
    {
        if (isCasting) return;

        for (int i = 0; i < activeSkills.Count && i < skillKeys.Length; i++)
        {
            if (Input.GetKeyDown(skillKeys[i]))
            {
                ActiveSkillBase activeSkill = activeSkills[i];
                if (activeSkill != null)
                {
                    bool executed = activeSkill.TryUse(gameObject, CreateSkillAnchor());
                    if (executed)
                    {
                        AnimationType animType = activeSkill.GetSkillAnimation();
                        if (activeSkill.castClip != null)
                        {
                            skillAudioSource.PlayOneShot(activeSkill.castClip);
                        }
                        else
                        {
                            Debug.Log("cast clip is null");
                        }

                        if (animType == AnimationType.Straight)
                        {
                            float castTime = activeSkill.GetCastTime();
                            playerAnimation.PlayStraightFor(castTime);
                        }
                        else
                        {
                            playerAnimation.SetAnimation(animType);
                        }

                        float lockDuration = activeSkill.GetPrepareTime() + activeSkill.GetCastTime();
                        if (lockDuration > 0f)
                            StartCoroutine(LockSkillInputCoroutine(lockDuration));
                    }
                }
            }
        }
    }

    // 시전 중 일정 시간 동안 스킬 입력을 잠그는 코루틴
    private IEnumerator LockSkillInputCoroutine(float duration)
    {
        isCasting = true;
        yield return new WaitForSeconds(duration);
        isCasting = false;
    }

    // 액티브 스킬 추가 (획득 시 호출)
    public void AddActiveSkill(ActiveSkillBase newSkill)
    {
        if (newSkill == null) return;

        bool isNew = !activeSkills.Contains(newSkill);

        if (isNew)
        {
            activeSkills.Add(newSkill);

            newSkill.Initialize();
            newSkill.InitializeCooldown();

            int idx = activeSkills.Count - 1;
            UpdateSkillIcon(idx);
            if (SkillPanel.Instance == null)
            {
                Init();
                Debug.Log("SkillPanel is null");
            }
            SkillPanel.Instance.OnLearnActiveSkill(newSkill);
        }

        // 🔹 덱에 넣는 건 "완전 신규 획득"일 때만
        if (ownedActiveDeck != null && isNew)
            ownedActiveDeck.AddRuntimeCard(newSkill);
    }

    // 덱은 건드리지 않고, 장착 목록 + UI만 관리
    public void AddActiveSkillToListAndUI(ActiveSkillBase newSkill)
    {
        if (newSkill == null) return;

        if (!activeSkills.Contains(newSkill))
        {
            activeSkills.Add(newSkill);

            newSkill.Initialize();
            newSkill.InitializeCooldown();

            int idx = activeSkills.Count - 1;
            UpdateSkillIcon(idx);
            if (SkillPanel.Instance == null)
            {
                Init();
                Debug.Log("SkillPanel is null");
            }
            SkillPanel.Instance.OnLearnActiveSkill(newSkill);
        }
    }



    // 액티브 스킬 제거
    public void RemoveAcvtiveSkill(ActiveSkillBase skill)
    {
        if (activeSkills.Contains(skill))
            activeSkills.Remove(skill);
    }

    // 패시브 스킬 추가
    public void AddPassiveSkill(PassiveSkillBase newSkill)
    {
        if (newSkill == null) return;

        // 중복 획득 어떻게 처리할지 생각하기
        //if (!passiveSkills.Contains(skill))
        //    passiveSkills.Add(skill);
        passiveSkills.Add(newSkill);
        if (SkillPanel.Instance == null)
        {
            Debug.Log("skill panel instance is null");
            return;
        }
        SkillPanel.Instance.OnLearnPassiveSkill(newSkill);

        if (newSkill is PS_AddHitEffectType addHitSkill)
        {
            foreach (var effSO in addHitSkill.hitEffects)
                runtimeEffects.Add(effSO.CreateEffectInstance());
        }
        if (newSkill is PS_AddStatType addStatSkill)
        {
            buffApplier.ApplyBuff(addStatSkill.buffStatType, addStatSkill.amount);
        }
    }


    // 스킬 적중 발생시 호출
    public void OnHit(HitContext ctx)
    {
        foreach (var eff in runtimeEffects)
            if (eff.CanApply(ctx))
                eff.Apply(ctx);
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

            string cooldownText = remainingCooldown >= 1f
                ? Mathf.Floor(remainingCooldown).ToString()
                : remainingCooldown.ToString("F1");

            cooldownTexts[skillIndex].text = cooldownText;

            if (remainingCooldown <= 0f)
            {
                cooldownTexts[skillIndex].gameObject.SetActive(false);
            }
            else
            {
                cooldownTexts[skillIndex].gameObject.SetActive(true);
            }

            if (skillIndex >= 0 && skillIndex < skillSlots.Count)
            {
                skillSlots[skillIndex].SetCooldownRatio(activeSkills[skillIndex].GetCooldownRatio());
            }
        }
    }


    // 바라보는 방향에 가장 먼저 맞은 곳에 프리팹을 생성해서 그 트랜스폼을 리턴함.
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

            if (hit.collider.CompareTag("Monster") || hit.collider.CompareTag("Boss"))
            {
                targetTransform = hit.transform.root;
            }

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
        if (skillSlots == null || skillSlots.Count == 0) return;
        if (skillIndex < 0 || skillIndex >= skillSlots.Count) return;
        if (skillIndex >= activeSkills.Count) return;

        var skill = activeSkills[skillIndex];
        if (skill == null) return;

        var icon = skill.GetIcon();
        if (icon == null)
        {
            Debug.Log("아이콘 발견 안됨.");
            return;
        }

        var slot = skillSlots[skillIndex];
        Debug.Log($"slot: {slot}, skill: {skill}, icon: {icon}");
        slot.SetIcon(icon);
    }

    // 액티브 스킬들 초기화
    public void InitalizeActiveSkills()
    {
        foreach (var skill in activeSkills)
        {
            if (skill != null)
                skill.Initialize();
        }
    }

    // ===================== 덱 기반 스킬 뽑기 API (UI에서 호출) ============================================================

    [Header("장착된 액티브 스킬 목록")]
    [SerializeField] private List<ActiveSkillBase> activeSkills = new List<ActiveSkillBase>();
    public List<ActiveSkillBase> GetActiveSkills() => activeSkills;
    public int GetNumOfActiveSkills() => activeSkills.Count;

    [Header("장착된 패시브 스킬 목록")]
    [SerializeField] private List<PassiveSkillBase> passiveSkills = new List<PassiveSkillBase>();
    public List<PassiveSkillBase> GetPassiveSkills() => passiveSkills;

    [Header("스킬 덱 (ScriptableObject)")]
    [SerializeField] private ActiveSkillDeckSO allActiveDeck;    // 전체 액티브 스킬 덱 (신규 뽑기용, 비복원)
    [SerializeField] private ActiveSkillDeckSO ownedActiveDeck;  // 획득한 액티브 스킬 덱 (중복 뽑기용, 복원)
    [SerializeField] private PassiveSkillDeckSO passiveSkillDeck; // 패시브 스킬 덱
    // 패시브 스킬 획득 횟수 (런타임 전용)
    private readonly Dictionary<PassiveSkillBase, int> passiveAcquireCounts = new();


    // 신규 액티브 스킬 한 장 뽑기 (미리보기용, 비복원 후보)
    public ActiveSkillBase PreviewNewActiveSkillFromDeck()
    {
        if (allActiveDeck == null) return null;
        // 덱에서 제거하지 말고 그냥 랜덤으로 하나 보기만
        var skill = allActiveDeck.GetRandomFromRuntime();
        return skill;
    }

    // 중복 액티브 스킬 한 장 뽑기 (복원, 획득 덱에서, 역시 미리보기)
    public ActiveSkillBase PreviewDuplicateActiveSkillFromDeck()
    {
        if (ownedActiveDeck == null) return null;

        var skill = ownedActiveDeck.DrawWithReplacementFromRuntime();
        return skill;
    }

    // 자동: 액티브 스킬 4종 전까지는 신규, 이후에는 중복 (전부 "미리보기")
    // isDuplicate = true 이면 중복 업그레이드 카드
    public ActiveSkillBase PreviewActiveSkillAutoFromDeck(out bool isDuplicate)
    {
        Debug.Log("Active Skill count : " + activeSkills.Count.ToString());

        if (activeSkills.Count < 4)
        {
            isDuplicate = false;
            return PreviewNewActiveSkillFromDeck();
        }
        else
        {
            isDuplicate = true;
            return PreviewDuplicateActiveSkillFromDeck();
        }
    }

    //  액티브 스킬 선택 확정 시 호출
    public void CommitActiveSkillSelection(ActiveSkillBase skill)
    {
        if (skill == null) return;

        // 아직 장착된 액티브 스킬이 4개 미만 → 신규 획득
        if (activeSkills.Count < 4)
        {
            // 전체 덱에서 실제로 제거 (비복원)
            allActiveDeck.RemoveRuntimeCard(skill);

            // 획득/중복 덱에 추가
            ownedActiveDeck.AddRuntimeCard(skill);

            // 처음 얻을 때는 1성으로 초기화
            skill.ClearStar();
        }
        else
        {
            // 이미 스킬 4개 이상 → 중복 강화
            skill.IncreaseStar();   // 내부에서 최대 3성까지 캡

            // ⭐ 3성에 도달하면, 중복 액티브 덱에서 제거
            if (skill.GetNumOfStar() >= 3 && ownedActiveDeck != null)
            {
                ownedActiveDeck.RemoveRuntimeCard(skill);
            }
        }

        // 🔹 이제 여기서는 목록/아이콘/패널만 갱신
        AddActiveSkillToListAndUI(skill);
    }

    /// <summary>
    /// 액티브 4종을 모두 획득한 이후,
    /// 중복 액티브 덱 + 패시브 덱을 하나로 합쳐서
    /// "이번 카드가 액티브가 될지"를 결정한다.
    /// </summary>
    public bool ShouldDrawActiveFromCombinedDeck()
    {
        int activeCount = 0;
        int passiveCount = 0;

        // 중복 액티브 덱에서 여전히 뽑을 수 있는 카드 수 (3성은 Commit에서 제거됨)
        if (ownedActiveDeck != null)
            activeCount = ownedActiveDeck.RuntimeCount;

        // 패시브 덱에서 여전히 뽑을 수 있는 카드 수
        if (passiveSkillDeck != null)
            passiveCount = passiveSkillDeck.DrawableCount;

        int total = activeCount + passiveCount;
        if (total <= 0)
            return false;   // 의미상 패시브 선택 (어차피 둘 다 없으면 나중에 null)

        int index = Random.Range(0, total);
        // 0 ~ activeCount-1 → 액티브, 나머지 → 패시브
        return index < activeCount;
    }


    // ---패시브 스킬---

    // 패시브 스킬 한 장 뽑기 (미리보기용)
    // - 각 패시브의 MaxAcquireCount와 현재 획득 횟수를 고려하여
    //   더 이상 획득 불가능한 카드는 뽑지 않는다.
    public PassiveSkillBase PreviewPassiveSkillFromDeck()
    {
        if (passiveSkillDeck == null) return null;

        const int maxTry = 30;
        PassiveSkillBase candidate = null;

        for (int i = 0; i < maxTry; i++)
        {
            candidate = passiveSkillDeck.DrawWithReplacement();
            if (candidate == null)
                return null;

            int max = candidate.MaxAcquireCount;
            int current = 0;
            passiveAcquireCounts.TryGetValue(candidate, out current);

            // max <= 0 이면 '무제한'으로 처리
            if (max <= 0 || current < max)
            {
                // 아직 획득 여유가 있는 카드면 사용
                return candidate;
            }

            // 이미 최대 횟수에 도달한 카드면 다시 뽑기 시도
            candidate = null;
        }

        // 모든 카드가 제한에 걸려있는 경우 등
        return candidate;
    }

    // 패시브 스킬 드로우
    public PassiveSkillBase DrawPassiveSkillFromDeck()
    {
        return PreviewPassiveSkillFromDeck();
    }


    // 패시브 스킬 선택 확정 시 호출.
    /// - 획득 횟수 증가
    /// - 최대 횟수에 도달하면 덱에서 제거
    /// - 실제 보유 패시브 목록에 반영 (AddPassiveSkill)
    public void CommitPassiveSkillSelection(PassiveSkillBase skill)
    {
        if (skill == null) return;

        // 현재까지 획득 횟수
        int current = 0;
        passiveAcquireCounts.TryGetValue(skill, out current);
        current++;
        passiveAcquireCounts[skill] = current;

        int max = skill.MaxAcquireCount;

        // max > 0 이고, 횟수가 다 찼으면 덱에서 제거
        if (max > 0 && current >= max && passiveSkillDeck != null)
        {
            passiveSkillDeck.RemoveCard(skill);
        }

        // 실제 패시브 스킬 보유 목록에 반영 (기존에 쓰던 로직)
        AddPassiveSkill(skill);
    }

    // 패시브 스킬 획득 횟수 조회
    public int GetPassiveAcquireCount(PassiveSkillBase skill)
    {
        if (skill == null) return 0;
        if (passiveAcquireCounts.TryGetValue(skill, out int count))
            return count;
        return 0;
    }


    // ===============================================테스트 함수들======================================================
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            if (Camera.main == null) return;

            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;
            Vector3 up = Camera.main.transform.up;

            Vector3 center = transform.position + forward * frontDistance;

            if (shotPos_LeftDown != null)
                shotPos_LeftDown.position = center + (-right - up).normalized * radius;

            if (shotPos_Left != null)
                shotPos_Left.position = center + (-right) * radius;

            if (shotPos_LeftUp != null)
                shotPos_LeftUp.position = center + (-right + up).normalized * radius;

            if (shotPos_Up != null)
                shotPos_Up.position = center + (up) * radius;

            if (shotPos_RightUp != null)
                shotPos_RightUp.position = center + (right + up).normalized * radius;

            if (shotPos_Right != null)
                shotPos_Right.position = center + (right) * radius;

            if (shotPos_RightDown != null)
                shotPos_RightDown.position = center + (right - up).normalized * radius;
        }
    }

    // Start()에서 호출되어야 하는 테스트 함수들의 집합
    public void TestMethodsInStart()
    {
        LoadInitialPassiveSkills();
    }

    // Update()에서 호출되어야 하는 테스트 함수들의 집합
    public void TestMethodsInUpdate()
    {
        ChangeProjectileAttributesForTest();
        IncreaseActiveSkillsStar();
    }

    public void IncreaseActiveSkillsStar()
    {
        if (Input.GetKeyDown(KeyCode.Alpha6))
            foreach (var skill in activeSkills)
            {
                if (skill != null)
                    skill.IncreaseStar();
            }
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
            r.material.color = new Color(1f, 1f, 0f, 0.4f);
            Destroy(marker.GetComponent<Collider>());
        }
    }

    // 테스트용 투사체 개수 변화
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

    // 씬에 배치된 초기 패시브 스킬 자동 등록
    public void LoadInitialPassiveSkills()
    {
        runtimeEffects.Clear();

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
#endif

}
