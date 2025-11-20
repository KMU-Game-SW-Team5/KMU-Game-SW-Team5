using UnityEngine;
using System.Collections;
using System.Collections.Generic; // List<>를 사용하기 위해 반드시 필요합니다.


public enum RoomType {Start, Normal, Boss}
public enum RoomState {Cleared, Uncleared}

public class RoomManager : MonoBehaviour
{
    [Header("방 상태 정보")]
    public RoomType type; // 방의 종류
    public RoomState state; // 클리어 여부


    [Header("몬스터 정보")]
    public List<GameObject> liveMonsters = new List<GameObject>();
    private bool isSpawned = false; // 몬스터가 이미 스폰되어 있는지 확인 확인하는 플래그
    private BoxCollider roomCollider;           // 플레이어 감지용 콜라이더


    public void Setup (RoomType newType)
    {
        type = newType; 

        // 시작 방은 클리어 상태로 둔다.
        if(type == RoomType.Start)
        {   
            state = RoomState.Cleared;
        } else
        {
            state = RoomState.Uncleared;
        }

        // 디버깅용 색상 변경 (나중에 제거 가능)
        if (type == RoomType.Start) GetComponent<Renderer>().material.color = Color.green;
        if (type == RoomType.Boss) GetComponent<Renderer>().material.color = Color.red;

        // Trigger Collider 설정 (방 크기에 맞춰 자동 조절)
        SetupCollider();
    }

        void SetupCollider()
    {
        // 사용자가 프리팹에 이미 넣어둔 BoxCollider를 가져옵니다.
        roomCollider = GetComponent<BoxCollider>();
        // 플레이어 감지용이므로 Trigger는 켜줍니다
        roomCollider.isTrigger = true; 
    }


    // Player 방에 입장시 호출 될 함수
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log($"{type} 방 입장! 상태: {state}");

            if (!isSpawned && type == RoomType.Normal)  // 방에 몬스터가 스폰된적 없으며, 방이 몬스터가 생성되는 방일때 호출 되는 조건문
            {
                SpawnMonsters();
            }
        }
    }

        public void SpawnMonsters()
    {
        isSpawned = true;

        // Resources/Monster 폴더 내의 모든 프리팹 로드
        GameObject[] monsterPrefabs = Resources.LoadAll<GameObject>("Monster");

        int monsterCount = Random.Range(2, 5); // 몬스터 생성되는 숫자 이후에 MapMaker에서 함수 호출시 바꿀 수 있도록 변경하기

        for (int i = 0; i < monsterCount; i++)
        {
            // ★ 반복문 안에서 매번 새로운 랜덤 몬스터를 선택합니다.
            GameObject selectedMonster = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];

            // 방 scale에 맞춰서 랜덤 생성된다.
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

    // ★ Step 4 추가: 몬스터가 죽었을 때 호출되는 함수
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

    void RoomClear()
    {
        state = RoomState.Cleared;
        Debug.Log("★ 방 클리어! 문이 열립니다. ★");
        
        // TODO: 다음 단계에서 여기에 문을 여는 로직(UnlockDoors) 추가 예정
        // GetComponentInChildren<DoorController>()?.OpenAllDoors();
    }
}
