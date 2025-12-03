using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI; // [필수 추가] NavMesh 관련 기능을 위해 추가

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
            SetPortalActive(col, true);
        }
    }

    // 자식 오브젝트 중 문 콜라이더의 isTrigger 활성화 (지나가게 열어줌)
    void UnlockDoors()
    {
        foreach (Collider col in doorColliders)
        {
            col.isTrigger = true; // 통과 가능 (해제)
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

        int selectedIndex = Random.Range(0, maxIndex + 1);
        GameObject selectedBoss = bossPrefabs[selectedIndex];

        // [핵심 변경] 보스 스폰 위치를 NavMesh 위로 보정
        // 방 중앙(transform.position) 주변 2.0f 반경 내의 안전한 바닥을 찾습니다.
        Vector3 spawnPos = GetValidSpawnPosition(transform.position, 2.0f);

        GameObject bossInstance = Instantiate(selectedBoss, spawnPos, Quaternion.identity);

        // 보스는 보통 크기가 커서 맵의 자식으로 넣으면 스케일 문제가 생길 수 있어 부모 설정 생략 권장
        // bossInstance.transform.SetParent(transform); 

        liveMonsters.Add(bossInstance);

        Debug.Log($"보스 스폰 완료: {bossInstance.name} (위치: {spawnPos})");
    }


    public void SpawnMonsters()
    {
        if (isSpawned) return; // 중복 스폰 방지
        isSpawned = true;

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
                0, // 원래 코드에 있던 고정 높이값
                Random.Range(-range, range)
            );

            // [복구됨] 로컬 좌표를 월드 좌표로 변환
            Vector3 spawnPos = transform.TransformPoint(randomPos);

            GameObject monster = Instantiate(selectedMonster, spawnPos, Quaternion.identity);

            // [복구됨] 자식으로 등록
            monster.transform.SetParent(transform);

            // 몬스터 리스트 추가
            liveMonsters.Add(monster);
        }
        Debug.Log($"{monsterCount}마리의 몬스터 스폰 완료.");
    }

    /// <summary>
    /// [추가된 기능] 주어진 중심점과 반경 내에서 NavMesh(이동 가능한 바닥) 위의 유효한 좌표를 반환합니다.
    /// 천장이나 장애물 위 스폰을 방지합니다.
    /// </summary>
    private Vector3 GetValidSpawnPosition(Vector3 center, float range)
    {
        int maxAttempts = 30; // 최대 시도 횟수
        for (int i = 0; i < maxAttempts; i++)
        {
            // 1. 중심 기준 랜덤 좌표 생성
            Vector3 randomPoint = center + Random.insideUnitSphere * range;

            // Y축은 방의 현재 바닥 높이로 고정 (천장 스폰 방지 핵심)
            randomPoint.y = center.y;

            NavMeshHit hit;
            // 2. 해당 좌표 근처(5.0f)에 NavMesh가 있는지 확인
            // NavMesh.AllAreas는 모든 구워진 NavMesh 영역을 의미합니다.
            if (NavMesh.SamplePosition(randomPoint, out hit, 5.0f, NavMesh.AllAreas))
            {
                // 유효한 위치를 찾았다면 그 위치 반환
                return hit.position;
            }
        }

        // 유효한 위치를 못 찾으면 방의 중심 반환 (최후의 수단)
        return center;
    }

    public void NotifyMonsterDied(GameObject monster)
    {
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
    }
}