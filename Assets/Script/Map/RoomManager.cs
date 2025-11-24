using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List<>를 사용하기 위해 반드시 필요합니다.

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
    private BoxCollider roomCollider;           // 플레이어 감지용 콜라이더
    private int minMonsterCount;
    private int maxMonsterCount;

    [Header("문 관리(자식 오브젝트)")]
    private List<Collider> doorColliders = new List<Collider>();    // 방의 자식으로 존재하는 문들을 저장할 리스트

    public void Setup(RoomType newType, int minMonsters, int maxMonsters)
    {
        type = newType;
        minMonsterCount = minMonsters;
        maxMonsterCount = maxMonsters;

        // 시작 방은 클리어 상태로 둔다.
        if (type == RoomType.Start)
        {   
            state = RoomState.Cleared;
        }
        else
        {
            state = RoomState.Uncleared;
        }

        // 몬스터 나오는 방에만 방 모듈 적용
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
            // 방 자체의 콜라이더(자기 자신)는 제외
            // if (col == roomCollider) continue;

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
            // 이름에 "MagicFortal"이 포함되어 있으면 (MagicFortal (1) 등도 포함)
            if (child.name.Contains("MagicFortal"))
            {
                child.gameObject.SetActive(isActive);

                // 포탈이 활성화될 때 방 타입이 Boss라면 색상 변경 
                if (isActive && type == RoomType.Boss)
                {
                    // 1. 파티클 시스템 색상 변경
                    ParticleSystem ps = child.GetComponent<ParticleSystem>();
                    if (ps != null)
                    {
                        var main = ps.main;
                        main.startColor = Color.red; // 빨간색으로 변경
                    }

                    // 2. (선택사항) 자식에 있는 Light 색상도 변경 (더 붉은 분위기 연출)
                    Light portalLight = child.GetComponentInChildren<Light>();
                    if (portalLight != null)
                    {
                        portalLight.color = Color.red;
                    }
                }
                
                // 혹시 파티클이 꺼져있을 수 있으니 확실하게 재생/정지 (선택사항)
                if (isActive)
                {
                    var ps = child.GetComponent<ParticleSystem>();
                    if (ps != null && !ps.isPlaying) ps.Play();
                }
            }
        }
    }

    // void SetupCollider()
    // {
    //     roomCollider = GetComponent<BoxCollider>();
        
    //     // 콜라이더가 없다면 생성
    //     if (roomCollider == null)
    //     {
    //         roomCollider = gameObject.AddComponent<BoxCollider>();
    //         roomCollider.size = new Vector3(10, 10, 10); 
    //         roomCollider.center = new Vector3(0, 5, 0);
    //     }
        
    //     // 플레이어 감지용이므로 Trigger는 켜줍니다
    //     roomCollider.isTrigger = true; 
    // }

    // Player 방에 입장 시 호출 될 함수
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{type} 방 입장! 상태: {state}");

            if (!isSpawned && type == RoomType.Normal)
            {
                LockDoors();
                SpawnMonsters();
            }
        }
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
                1, 
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
    }
}