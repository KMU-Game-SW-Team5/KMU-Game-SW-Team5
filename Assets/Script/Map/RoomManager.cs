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

    // 방의 설정 값을 정의 하는 함수 -> MapMaker 함수에서 호출
    // [변경] difficulty 매개변수 추가
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

        // 방모듈중에서 방을 무작위로 선택한다.
        int randomIndex = Random.Range(0, modules.Length);
        GameObject selectedModule = modules[randomIndex];

        // 방모듈을 Plane의 자식 오브젝트로 설정
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
        if(state == RoomState.Cleared)
        {
            UnlockDoors();
        }
        // UnCleared 방의 경우 포탈 활성화
        else
        {
            foreach(Collider col in doorColliders)
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
                    // 3. 그 외(일반방 평상시, 시작방 등)는 shouldChangeColor가 false이므로 색을 건드리지 않음

                    
                    // 파티클 시스템 처리
                    ParticleSystem[] allParticles = child.GetComponentsInChildren<ParticleSystem>(true);
                    foreach (ParticleSystem ps in allParticles)
                    {
                        // 색상을 바꿔야 하는 경우에만 색 변경 코드를 실행
                        if (shouldChangeColor)
                        {
                            var main = ps.main;
                            main.startColor = targetColor;
                        }

                        // 색 변경 여부와 상관없이 활성화됐으니 재생은 시킴
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
                // [변경] Normal 방 또는 Boss 방 입장 시 문 잠그고 몬스터/보스 소환
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

        // Resources/Boss 폴더 내의 모든 프리팹 로드
        GameObject[] bossPrefabs = Resources.LoadAll<GameObject>("Boss");

        if (bossPrefabs.Length == 0)
        {
            Debug.LogWarning("보스 프리팹을 찾을 수 없습니다. (Resources/Boss)");
            return;
        }

        // 난이도에 따른 인덱스 제한
        int maxIndex = 0;
        switch (gameDifficulty)
        {
            case 0: maxIndex = 0; break;     // Easy: 0
            case 1: maxIndex = 1; break;     // Normal: 0~1
            case 2: maxIndex = 2; break;     // Hard: 0~2
            default: maxIndex = 2; break;
        }

        // 배열 범위를 벗어나지 않도록 클램핑
        if (maxIndex >= bossPrefabs.Length) maxIndex = bossPrefabs.Length - 1;

        // 랜덤 선택 (maxIndex + 1은 exclusive이므로 포함시키려면 +1)
        int selectedIndex = Random.Range(0, maxIndex + 1);
        GameObject selectedBoss = bossPrefabs[selectedIndex];

        // [수정됨] 보스 소환 위치 계산
        // transform.position은 이미 World 좌표입니다. 여기에 높이(Y)만 살짝 더해줍니다.
        Vector3 spawnPos = transform.position;
        spawnPos.y += 30f; // 30.0f는 너무 높을 수 있어 2.0f로 조정 (보스 크기에 따라 조절)

        // [수정됨] TransformPoint 제거: 이미 World 좌표이므로 변환 불필요
        // [수정됨] SetParent 제거: 몬스터 찌그러짐 방지를 위해 부모 미설정
        GameObject bossInstance = Instantiate(selectedBoss, spawnPos, Quaternion.identity);
        
        // liveMonsters에 추가하여 죽음 감지 및 문 열림 로직과 연동
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

            // 방 scale에 맞춰서 랜덤 생성
            float range = 4.0f; 
            Vector3 randomPos = new Vector3(
                Random.Range(-range, range), 
                60, 
                Random.Range(-range, range)
            );
            
            // 로컬 좌표를 월드 좌표로 변환
            Vector3 spawnPos = transform.TransformPoint(randomPos);

            GameObject monster = Instantiate(selectedMonster, spawnPos, Quaternion.identity);
            
            // 자식으로 등록
            monster.transform.SetParent(transform);
            
            // 몬스터 리스트 추가
            liveMonsters.Add(monster);
        }
        Debug.Log($"{monsterCount}마리의 몬스터 스폰 완료.");
    }

    public void NotifyMonsterDied(GameObject monster)
    {
        if (liveMonsters.Contains(monster))
        {   
            // 남은 몬스터에서 현재 제거된 몬스터를 삭제
            liveMonsters.Remove(monster);

            // 모든 몬스터를 잡았는지 확인
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

        // 보스방 클리어 시 추가 로직이 필요하면 여기에 작성
        if (type == RoomType.Boss)
        {
            Debug.Log("보스방 클리어!");
        }
    }
}