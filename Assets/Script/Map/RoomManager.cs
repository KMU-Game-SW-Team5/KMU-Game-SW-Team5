using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 방의 종류와 상태 정의
public enum RoomType { Start, Normal, Boss }
public enum RoomState { Cleared, Uncleared }

public class RoomManager : MonoBehaviour
{
    [Header("방 상태 정보")]
    public RoomType type;       // 방의 종류
    public RoomState state;     // 클리어 여부

    [Header("몬스터 정보")]
    public List<GameObject> liveMonsters = new List<GameObject>();
    private bool isSpawned = false;             // 몬스터가 이미 스폰되어 있는지 확인하는 플래그
    private int minMonsterCount;
    private int maxMonsterCount;
    private int gameDifficulty;                 // [추가] 난이도 저장

    [Header("문 관리(자식 오브젝트)")]
    private List<Collider> doorColliders = new List<Collider>();    // 방의 자식으로 존재하는 문들을 저장할 리스트

    [Header("BGM")]
    [SerializeField] private float nonCombatDelay = 10f; // 비전투 상태 유지 시간(초) 후 normal 재생

    private Coroutine nonCombatCoroutine;

    // 방의 설정 값을 정의 하는 함수 -> MapMaker 함수에서 호출
    public void Setup(RoomType newType, int minMonsters, int maxMonsters, int difficulty)
    {
        type = newType;
        minMonsterCount = minMonsters;
        maxMonsterCount = maxMonsters;
        gameDifficulty = difficulty;

        // 시작 방은 클리어 상태로 둔다.
        if (type == RoomType.Start)
        {
            state = RoomState.Cleared;
        }
        else
        {
            state = RoomState.Uncleared;
        }
    }


    // 맵 생성이 완전히 끝난 뒤 호출됩니다.
    public void Initialize()
    {
        // 몬스터 나오는 방에만 방 모듈 적용 (Normal일 때만)
        if (type == RoomType.Normal)
        {
            SpawnMapModule();
        }

        // 문 찾기 및 초기화
        FindLocalDoors();
    }

    // 방 구조물 랜덤 생성 함수
    void SpawnMapModule()
    {
        // Resources/MapModule 폴더의 모든 프리팹 로드
        GameObject[] modules = Resources.LoadAll<GameObject>("MapModule");

        if (modules.Length == 0) return;

        // 방모듈을 Plane의 자식 오브젝트로 설정
        // [수정 권장] 모듈 생성 시 부모의 스케일 영향을 줄이기 위해 로직 유지하되, 
        // 몬스터 생성 시에는 NavMesh 위를 찾도록 함.
        int randomIndex = Random.Range(0, modules.Length);
        GameObject selectedModule = modules[randomIndex];

        GameObject instance = Instantiate(selectedModule, transform.position, Quaternion.identity);
        instance.transform.SetParent(transform);
    }

    // 방 생성 시 초기 문의 상태를 열어둔 상태로 설정 및 문 찾기
    void FindLocalDoors()
    {
        doorColliders.Clear();

        Collider[] allColliders = GetComponentsInChildren<Collider>(true);

        foreach (Collider col in allColliders)
        {
            // 태그가 "Door"인 콜라이더만 리스트에 추가
            if (col.CompareTag("Door"))
            {
                doorColliders.Add(col);
            }
        }

        // Start 방의 경우 포탈 비활성화
        if (state == RoomState.Cleared)
        {
            UnlockDoors();
        }
        // UnCleared 방의 경우 포탈 활성화
        else
        {
            foreach (Collider col in doorColliders)
            {
                col.isTrigger = true;
                col.gameObject.layer = LayerMask.NameToLayer("Default"); // 문 레이어를 Default로 변경하여 투사체 막음
                SetPortalActive(col, true);
            }
        }
    }


    // 자식 오브젝트 중 문 콜라이더의 isTrigger 비활성화 (못 지나가게 막음)
    void LockDoors()
    {
        foreach (Collider col in doorColliders)
        {
            col.isTrigger = false; // 물리적 벽 (잠금)
            col.gameObject.layer = LayerMask.NameToLayer("Default"); // 문 레이어를 Default로 변경하여 투사체 막음
            SetPortalActive(col, true);
        }
    }

    // 자식 오브젝트 중 문 콜라이더의 isTrigger 활성화 (지나가게 열어줌)
    void UnlockDoors()
    {
        foreach (Collider col in doorColliders)
        {
            col.isTrigger = true; // 통과 가능 (해제)
            col.gameObject.layer = LayerMask.NameToLayer("Projectile"); // 문 레이어를 Projectile로 변경하여 투사체 통과 허용
            SetPortalActive(col, false);
        }
    }

    void SetPortalActive(Collider doorCol, bool isActive)
    {
        // 문(Door)의 자식들을 순회하며 MagicFortal을 찾습니다.
        foreach (Transform child in doorCol.transform)
        {
            // 이름에 "MagicFortal"이 포함되어 있으면
            if (child.name.Contains("MagicFortal"))
            {
                child.gameObject.SetActive(isActive);

                if (isActive)
                {
                    Color targetColor = Color.white; // 임시 초기값
                    bool shouldChangeColor = false;  // 색상을 바꿀지 말지 결정하는 플래그

                    // 1. 보스방: 언제나 빨간색 (최우선)
                    if (type == RoomType.Boss)
                    {
                        targetColor = Color.red;
                        shouldChangeColor = true;
                    }
                    // 2. 일반방: 문이 잠겼을 때만(전투 중) 보라색
                    else if (type == RoomType.Normal && !doorCol.isTrigger)
                    {
                        targetColor = new Color(0.6f, 0f, 1f); // 보라색
                        shouldChangeColor = true;
                    }


                    // 파티클 시스템 처리
                    ParticleSystem[] allParticles = child.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (ParticleSystem ps in allParticles)
                    {
                        if (shouldChangeColor)
                        {
                            var main = ps.main;
                            main.startColor = targetColor;
                        }
                        if (!ps.isPlaying) ps.Play();
                    }

                    // (선택사항) Light 처리
                    Light[] allLights = child.GetComponentsInChildren<Light>(true);
                    foreach (Light l in allLights)
                    {
                        if (shouldChangeColor)
                        {
                            l.color = targetColor;
                        }
                    }
                }
            }
        }
    }

    // Player 방에 입장 시 호출 될 함수
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 엔딩 대기 상태면 방 입장 무시
            if (GameManager.Instance != null && GameManager.Instance.IsEnding)
            {
                Debug.Log("게임 엔딩 대기 중 — 방 입장 처리 무시");
                return;
            }

            Debug.Log($"{type} 방 입장! 상태: {state}");

            // Minimap UI
            int gx = Mathf.RoundToInt(transform.position.x / (10 * 40));
            int gy = Mathf.RoundToInt(transform.position.z / (10 * 40));
            InGameUIManager.Instance.UpdateCurrentRoom(new Vector2Int(gx, gy));

            if (!isSpawned)
            {
                if (type == RoomType.Normal)
                {
                    LockDoors();
                    SpawnMonsters();
                }
                else if (type == RoomType.Boss)
                {
                    LockDoors();
                    SpawnBoss();
                }
            }
        }
    }

    // 보스 소환 로직
    public void SpawnBoss()
    {
        if (isSpawned) return;
        isSpawned = true;

        // 전투로 전환 (BGM)
        if (BGM_Manager.Instance != null)
        {
            BGM_Manager.Instance.PlayBoss();
        }

        // 중복된 non-combat 타이머가 있으면 취소
        CancelNonCombatTimer();

        GameObject[] bossPrefabs = Resources.LoadAll<GameObject>("Boss");

        if (bossPrefabs.Length == 0)
        {
            Debug.LogWarning("보스 프리팹을 찾을 수 없습니다. (Resources/Boss)");
            return;
        }

        int maxIndex = 0;
        switch (gameDifficulty)
        {
            case 0: maxIndex = 0; break;     // Easy: 0
            case 1: maxIndex = 1; break;     // Normal: 0~1
            case 2: maxIndex = 2; break;     // Hard: 0~2
            default: maxIndex = 2; break;
        }

        if (maxIndex >= bossPrefabs.Length) maxIndex = bossPrefabs.Length - 1;

        // 보스 순차적 생성
        GameObject selectedBoss = bossPrefabs[GameManager.Instance.bossIdx++];

        Vector3 spawnPos = transform.position;

        GameObject bossInstance = Instantiate(selectedBoss, spawnPos, Quaternion.identity);

        BossMonsterBase bossComponent = bossInstance.GetComponent<BossMonsterBase>();
        bossComponent.SetRoom(this);    // 보스 방 참조 전달
        // 보스의 난이도 설정: (총 보스 처치 수 + 1) * (게임 난이도 + 1)
        // 난이도당 체력과 공격력이 올라가는 정도는 각 보스에서 설정
        bossComponent.SetDifficulty((KillCounter.Instance.TotalBossKills + 1)
            * (gameDifficulty + 1));

        // 보스는 보통 크기가 커서 맵의 자식으로 넣으면 스케일 문제가 생길 수 있어 부모 설정 생략 권장
        //bossInstance.transform.SetParent(transform);

        liveMonsters.Add(bossInstance);

        Debug.Log($"보스 스폰 완료: {bossInstance.name} (위치: {spawnPos})");
    }


    public void SpawnMonsters()
    {
        if (isSpawned) return; // 중복 스폰 방지
        isSpawned = true;

        // 전투로 전환 (BGM)
        if (BGM_Manager.Instance != null)
        {
            BGM_Manager.Instance.PlayCombat();
        }

        // 중복된 non-combat 타이머가 있으면 취소
        CancelNonCombatTimer();

        // Resources/Monster 폴더 내의 모든 프리팹 로드
        GameObject[] monsterPrefabs = Resources.LoadAll<GameObject>("Monster");

        if (monsterPrefabs.Length == 0) return; // 몬스터가 없으면 리턴

        int monsterCount = Random.Range(minMonsterCount, maxMonsterCount);

        for (int i = 0; i < monsterCount; i++)
        {
            GameObject selectedMonster = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];

            // [복구됨] 방 scale에 맞춰서 랜덤 생성 (이미지의 빨간색 영역)
            float range = 1.0f;
            Vector3 randomPos = new Vector3(
                Random.Range(-range, range),
                1, // 원래 코드에 있던 고정 높이값
                Random.Range(-range, range)
            );

            // [복구됨] 로컬 좌표를 월드 좌표로 변환
            Vector3 spawnPos = transform.TransformPoint(randomPos);

            GameObject monster = Instantiate(selectedMonster, spawnPos, Quaternion.identity);
            MonsterBase monsterComponent = monster.GetComponent<MonsterBase>();
            monsterComponent.SetDifficulty((KillCounter.Instance.TotalBossKills + 1)
                * (gameDifficulty + 1));

            // [복구됨] 자식으로 등록
            monster.transform.SetParent(transform);

            // 몬스터 리스트 추가
            liveMonsters.Add(monster);
        }
        Debug.Log($"{monsterCount}마리의 몬스터 스폰 완료.");
    }


    public void NotifyMonsterDied(GameObject monster)
    {
        Debug.Log($"몬스터 사망 알림 받음: {monster.name}");
        if (liveMonsters.Contains(monster))
        {
            liveMonsters.Remove(monster);

            if (liveMonsters.Count == 0)
            {
                RoomClear();
            }
        }
    }

    // 방 클리어 시 호출
    void RoomClear()
    {
        state = RoomState.Cleared;
        UnlockDoors();

        if (type == RoomType.Boss)
        {
            Debug.Log("보스방 클리어!");
        }

        // 비전투 상태 타이머 시작 (다른 방에서 전투가 시작되면 해당 방에서 호출한 PlayCombat이 우선 적용됨)
        StartNonCombatTimer();
    }

    // -----------------------
    // non-combat 타이머 관련
    // -----------------------
    private void StartNonCombatTimer()
    {
        CancelNonCombatTimer();
        nonCombatCoroutine = StartCoroutine(NonCombatTimerCoroutine());
    }

    private void CancelNonCombatTimer()
    {
        if (nonCombatCoroutine != null)
        {
            StopCoroutine(nonCombatCoroutine);
            nonCombatCoroutine = null;
        }
    }

    private IEnumerator NonCombatTimerCoroutine()
    {
        float timer = 0f;
        while (timer < nonCombatDelay)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // 비전투 10초 후, 현재 씬에 살아있는 몬스터가 없으면 normal BGM 재생(엔딩 제외)
        if (!AnyLiveMonstersInScene())
        {
            if (BGM_Manager.Instance != null && !BGM_Manager.Instance.IsPlayingEndingMusic())
            {
                BGM_Manager.Instance.PlayNormal();
            }
        }

        nonCombatCoroutine = null;
    }

    // 현재 씬의 모든 RoomManager를 확인해 살아있는 몬스터가 있는지 검사
    private bool AnyLiveMonstersInScene()
    {
        RoomManager[] rooms = FindObjectsOfType<RoomManager>();
        foreach (var rm in rooms)
        {
            if (rm == null) continue;
            if (rm.liveMonsters != null && rm.liveMonsters.Count > 0)
                return true;
        }
        return false;
    }
}