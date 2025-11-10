using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    public int n = 10;              // 생성할 방의 개수
    public int roomSize = 1;        // 방 크기
    public GameObject floor;        // 바닥 프리팹
    public GameObject wall;         // 벽 프리팹 
    public GameObject wallDoor;     // 문이 있는 벽 프리팹 
    public GameObject boss;         // 보스몹 프리팹

    private HashSet<Vector3> TempSet = new HashSet<Vector3>();
    private HashSet<Vector3> PositionSet = new HashSet<Vector3>();
    private HashSet<Vector3> WallSet = new HashSet<Vector3>();
    private Queue<Vector3> PositionQueue = new Queue<Vector3>();
    private Vector3 nextPosition;   // 새로 생성될 방 위치
    private Vector3 bossPosition;

    void Start()
    {
        FloorMaker();
        WallMaker();
        BossRoomMaker();

        // 맵 생성이 모두 끝난 시점
        Debug.Log("맵 생성 완료!");


    }

    void FloorMaker()
    {   
        // 큐와 집합 초기화 및 초기값 설정
        PositionQueue.Clear();
        TempSet.Clear();
        PositionQueue.Enqueue(new Vector3(0, 0, 0));
        TempSet.Add(new Vector3(0, 0, 0));
        n--;

        while (PositionQueue.Count != 0)
        {
            // 큐에서 위치 꺼내기
            Vector3 nowPosition = PositionQueue.Dequeue();
            bossPosition = nowPosition;

            // 방생성
            GameObject newfloor = Instantiate(floor, nowPosition, Quaternion.identity);
            newfloor.transform.localScale = new Vector3(roomSize, 1, roomSize);

            // 방이 생성된 위치를 집합에 저장
            PositionSet.Add(nowPosition);

            // 상하좌우 방향 리스트
            List<string> UDLRlist = new List<string> { "Up", "Down", "Left", "Right" };

            bool FirstAccess = true;

            for (int i = 0; i < 4; i++)
            {
                // 무작위 인덱스 선택
                int index = Random.Range(0, UDLRlist.Count);
                string selected = UDLRlist[index];

                // q방이 생성될 위치를 nextPosition에 생성
                switch (selected)
                {
                    case "Up":
                        nextPosition = new Vector3(nowPosition.x, 0, nowPosition.z + (10 * roomSize));
                        break;
                    case "Down":
                        nextPosition = new Vector3(nowPosition.x, 0, nowPosition.z - (10 * roomSize));
                        break;
                    case "Left":
                        nextPosition = new Vector3(nowPosition.x - (10 * roomSize), 0, nowPosition.z);
                        break;
                    case "Right":
                        nextPosition = new Vector3(nowPosition.x + (10 * roomSize), 0, nowPosition.z);
                        break;
                }

                // 사용한 방향 제거
                UDLRlist.RemoveAt(index);

                // 접근한적 없는 위치인 경우 생성할 수 있는 방의 개수가 남아 있는 경우
                if (n != 0 && !TempSet.Contains(nextPosition))
                {
                    if (FirstAccess)
                    {
                        n--;
                        PositionQueue.Enqueue(nextPosition);
                        TempSet.Add(nextPosition);
                        FirstAccess = false;
                    }
                    else
                    {
                        if (Random.value < 0.5f)
                        {
                            n--;
                            PositionQueue.Enqueue(nextPosition);
                            TempSet.Add(nextPosition);
                        }
                    }
                }

            }

        }
    }

    void WallMaker()
    {
        // 큐와 집합 초기화 및 초기값 설정
        PositionQueue.Clear();
        TempSet.Clear();
        PositionQueue.Enqueue(new Vector3(0, 0, 0));
        TempSet.Add(new Vector3(0, 0, 0));

        while (PositionQueue.Count != 0)
        {
            // 벽을 생성할 방의 좌표를 얻는다
            Vector3 nowPosition = PositionQueue.Dequeue();
            Vector3 wallPosition = new Vector3(0, 0, 0);
            Quaternion wallRotation = Quaternion.Euler(0, 0, 0);

            // 상하좌우 방향 리스트
            List<string> UDLRlist = new List<string> { "Up", "Down", "Left", "Right" };

            // 첫번째 접근 하는 방을 확인하기 위한 flag
            bool FirstAccess = true;

            for (int i = 0; i < 4; i++)
            {
                // 무작위 인덱스 선택
                int index = Random.Range(0, UDLRlist.Count);
                string selected = UDLRlist[index];

                // 벽을 만들 방향에 옆방이 존재 하는지 아닌지 확인
                switch (selected)
                {
                    case "Up":
                        nextPosition = new Vector3(nowPosition.x, 0, nowPosition.z + (10 * roomSize));
                        wallPosition = nowPosition + new Vector3(0, 0, ((10 * roomSize) / 2.0f));
                        wallRotation = Quaternion.Euler(0, 90, 0);
                        break;
                    case "Down":
                        nextPosition = new Vector3(nowPosition.x, 0, nowPosition.z - (10 * roomSize));
                        wallPosition = nowPosition + new Vector3(0, 0, -((10 * roomSize) / 2.0f));
                        wallRotation = Quaternion.Euler(0, -90, 0);
                        break;
                    case "Left":
                        nextPosition = new Vector3(nowPosition.x - (10 * roomSize), 0, nowPosition.z);
                        wallPosition = nowPosition + new Vector3(-((10 * roomSize) / 2.0f), 0, 0);
                        wallRotation = Quaternion.Euler(0, 0, 0);
                        break;
                    case "Right":
                        nextPosition = new Vector3(nowPosition.x + (10 * roomSize), 0, nowPosition.z);
                        wallPosition = nowPosition + new Vector3(((10 * roomSize) / 2.0f), 0, 0);
                        wallRotation = Quaternion.Euler(0, 180, 0);
                        break;
                }

                // 사용한 방향 제거
                UDLRlist.RemoveAt(index);

                // 방이 존재하는 방향이라면
                if (PositionSet.Contains(nextPosition))
                {
                    // TempSet에 없는 위치라면(처음 접근하는 방이라면 Queue에 추가) 
                    if (!TempSet.Contains(nextPosition))
                    {
                        TempSet.Add(nextPosition);
                        PositionQueue.Enqueue(nextPosition);
                        WallSet.Add(wallPosition); // 벽이 만들어진 위치 저장

                        // 처음 접근하는 것으로 문을 만든다.
                        GameObject doorWall = Instantiate(wallDoor, wallPosition, wallRotation);
                        doorWall.transform.localScale *= roomSize;
                    }
                    else // TempSet에 있는 위치라면(이미 접근한 적 있는 방이라면)
                    {
                        if (!WallSet.Contains(wallPosition))    // 옆방과 현재 방 사이에 벽이 없는 경우
                        {
                            WallSet.Add(wallPosition);

                            if (Random.value < 0.2f) // 20% 확률로 벽을 만든다. 
                            {
                                // 처음 접근하는 것으로 문을 만든다.
                                GameObject doorWall = Instantiate(wallDoor, wallPosition, wallRotation);
                                doorWall.transform.localScale *= roomSize;
                            }
                            else
                            {
                                // 문이 없는 벽 생성, 및 방 크기 변경
                                wallPosition.y += wall.transform.localScale.y * ((roomSize / 2));
                                GameObject newWall = Instantiate(wall, wallPosition, wallRotation);
                                newWall.transform.localScale *= roomSize;
                            }
                        }
                    }
                }
                else // 방을 생성할 수 있는 위치가 아니라면
                {
                    // 문이 없는 벽 생성, 및 방 크기 변경
                    wallPosition.y += wall.transform.localScale.y * ((roomSize / 2));
                    GameObject newWall = Instantiate(wall, wallPosition, wallRotation);
                    newWall.transform.localScale *= roomSize;
                }

            }
        }
    }

    void BossRoomMaker()
    {
        bossPosition.y += 20;
        GameObject newfloor = Instantiate(boss, bossPosition, Quaternion.identity);
    }
}
