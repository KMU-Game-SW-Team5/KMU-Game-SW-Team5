using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TempMapMaker : MonoBehaviour
{
    public int n = 10;              // 생성할 방의 개수
    public int floorSize = 1;       // 바닥 크기
    public GameObject floor;        // 바닥 프리팹
    public int wallSize = 1;        // 벽 크기 일단, floor Size와는 통일할 예정이다.
    public GameObject wall;

    // --- 변경된 내용 ---
    [Header("Room Settings")]
    public float roomDimension = 10f; // 방의 실제 크기 (10 * floorSize 대신 사용)
    public float roomTriggerHeight = 20f; // 방 트리거의 높이

    [Header("Monster Settings")]
    public GameObject TempMonster; // 몬스터 프리팹 (임시 캡슐)
    public int monstersPerRoom = 1;  // 방당 몬스터 스폰 수
    // -------------------------

    private HashSet<Vector3> PositionSet = new HashSet<Vector3>();
    private Queue<Vector3> PositionQueue = new Queue<Vector3>();

    private Vector3 currentPosition = new Vector3(0, 0, 0); // 시작 지점
    private Vector3 nextPosition;   // 새로 생성될 방 위치


    void Start()
    {
        // 맵 생성 함수 실행
        PlaneMaker();
    }

    void PlaneMaker()
    {
        // 시작 지점 초기화
        PositionQueue.Enqueue(currentPosition);
        PositionSet.Add(currentPosition);

        // 방 구분을 위한 카운터
        int roomCounter = 1;

        while (n > 0 && PositionQueue.Count > 0)
        {
            // 큐에서 위치 꺼내기
            currentPosition = PositionQueue.Dequeue();

            // 방 생성
            GameObject newfloor = Instantiate(floor, currentPosition, Quaternion.identity);
            newfloor.transform.localScale = new Vector3(floorSize, 1, floorSize);
            newfloor.name = $"Room_{roomCounter++}";
            n--;

            // 방에 RoomManager 스크립트 추가 및 초기화
            RoomManager roomManager = newfloor.AddComponent<RoomManager>();
            float actualRoomDimension = roomDimension * floorSize;
            roomManager.Initialize(actualRoomDimension);

            // 이 방에 몬스터 스폰
            for (int i = 0; i < monstersPerRoom; i++)
            {
                // 방 중앙에서 랜덤한 위치에 스폰
                float spawnRadius = (actualRoomDimension / 2) * 0.8f; // 가장자리를 피하기 위해 80% 반경
                Vector3 spawnPos = currentPosition + new Vector3(
                    Random.Range(-spawnRadius, spawnRadius),
                    10f, 
                    Random.Range(-spawnRadius, spawnRadius)
                );
                
                GameObject newMonster = Instantiate(TempMonster, spawnPos, Quaternion.identity);
                newMonster.name = $"Monster_{roomCounter-1}_{i+1}";
                
                // 생성된 몬스터를 방의 자식으로 설정 
                newMonster.transform.SetParent(newfloor.transform);
                
                // 몬스터AI를 RoomManager의 리스트에 추가
                MonsterAI monsterAI = newMonster.GetComponent<MonsterAI>();
                if (monsterAI != null)
                {
                    roomManager.monstersInRoom.Add(monsterAI);
                }
            }

            // 상하좌우 방향 리스트
            List<string> UDLRlist = new List<string> { "Up", "Down", "Left", "Right" };

            for (int i = 0; i < 4; i++)
            {
                // 무작위 인덱스 선택
                int index = Random.Range(0, UDLRlist.Count);
                string selected = UDLRlist[index];

                // 방향에 따른 위치 계산
                switch (selected)
                {
                    case "Up":
                        nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z + (10 * floorSize));
                        break;
                    case "Down":
                        nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z - (10 * floorSize));
                        break;
                    case "Left":
                        nextPosition = new Vector3(currentPosition.x - (10 * floorSize), 0, currentPosition.z);
                        break;
                    case "Right":
                        nextPosition = new Vector3(currentPosition.x + (10 * floorSize), 0, currentPosition.z);
                        break;
                }

                // 새로운 위치라면 50% 확률로 추가
                if (!PositionSet.Contains(nextPosition))
                {
                    if (Random.value < 0.5f)
                    {
                        PositionQueue.Enqueue(nextPosition);
                        PositionSet.Add(nextPosition);
                    }
                }

                // 사용한 방향 제거
                UDLRlist.RemoveAt(index);
            }
        }
    }

    void WallMaker()
    {
        
    }
}
