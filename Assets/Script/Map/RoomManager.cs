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

        // 디버깅용 색상 변경 (나중에 제거 가능)
        if (type == RoomType.Start) GetComponent<Renderer>().material.color = Color.green;
        if (type == RoomType.Boss) GetComponent<Renderer>().material.color = Color.red;

        // 문 찾기 및 초기화
        FindLocalDoors();

        // Trigger Collider 설정
        SetupCollider();
    }

    // 방 생성 시 초기 문의 상태를 열어둔 상태로 설정 및 문 찾기
    void FindLocalDoors()
    {
        doorColliders.Clear();

        Collider[] allColliders = GetComponentsInChildren<Collider>(true);

        // ★ 수정된 부분: allColliders 배열을 순회해야 합니다.
        foreach (Collider col in allColliders)
        {
            // 방 자체의 콜라이더(자기 자신)는 제외
            if (col == roomCollider) continue;

            // 태그가 "Door"인 콜라이더만 리스트에 추가
            if (col.CompareTag("Door"))
            {   
                Debug.Log("Door 콜라이더 추가");
                doorColliders.Add(col);
            }
        }

        // 초기 상태: 문 열기
        UnlockDoors();
    }


    // 자식 오브젝트 중 문 콜라이더의 isTrigger 비활성화 (못 지나가게 막음)
    void LockDoors()
    {
        foreach (Collider col in doorColliders)
        {
            if (col != null) 
            {
                col.isTrigger = false; // 물리적 벽 (잠금)
            }
        }
    }

    // 자식 오브젝트 중 문 콜라이더의 isTrigger 활성화 (지나가게 열어줌)
    void UnlockDoors()
    {
        foreach (Collider col in doorColliders)
        {
            if (col != null) 
            {
                col.isTrigger = true; // 통과 가능 (해제)
            }
        }
    }

    void SetupCollider()
    {
        roomCollider = GetComponent<BoxCollider>();
        
        // 콜라이더가 없다면 생성
        if (roomCollider == null)
        {
            roomCollider = gameObject.AddComponent<BoxCollider>();
            roomCollider.size = new Vector3(10, 10, 10); 
            roomCollider.center = new Vector3(0, 5, 0);
        }
        
        // 플레이어 감지용이므로 Trigger는 켜줍니다
        roomCollider.isTrigger = true; 
    }

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
            liveMonsters.Remove(monster);
            Debug.Log($"몬스터 처치! 남은 수: {liveMonsters.Count}");

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
        Debug.Log("★ 방 클리어! 문이 열립니다. ★");

        UnlockDoors();
    }
}