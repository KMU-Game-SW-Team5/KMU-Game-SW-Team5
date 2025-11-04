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

    private HashSet<Vector3> PositionSet = new HashSet<Vector3>();
    private Queue<Vector3> PositionQueue = new Queue<Vector3>();
    private Vector3 nextPosition;   // 새로 생성될 방 위치

    void Start()
    {
        FloorMaker();
    }

    void FloorMaker()
    {   
        // 큐와 집합 초기화 및 초기값 설정
        PositionQueue.Clear();
        PositionSet.Clear();
        PositionQueue.Enqueue(new Vector3(0, 0, 0));
        PositionSet.Add(new Vector3(0, 0, 0));

        while (PositionQueue.Count != 0)
        {
            // 큐에서 위치 꺼내기
            Vector3 nowPosition = PositionQueue.Dequeue();

            // 방생성
            GameObject newfloor = Instantiate(floor, currentPosition, Quaternion.identity);
            newfloor.transform.localScale = new Vector3(roomSize, 1, roomSize);

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
                        nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z + (10 * roomSize));
                        break;
                    case "Down":
                        nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z - (10 * roomSize));
                        break;
                    case "Left":
                        nextPosition = new Vector3(currentPosition.x - (10 * roomSize), 0, currentPosition.z);
                        break;
                    case "Right":
                        nextPosition = new Vector3(currentPosition.x + (10 * roomSize), 0, currentPosition.z);
                        break;
                }

                // 사용한 방향 제거
                UDLRlist.RemoveAt(index);

                // 접근한적 없는 위치인 경우
                if (!PositionSet.Contains(nextPosition) && n != 0)
                {
                    if (Random.value < 0.5f)
                    {
                        n--;
                        PositionQueue.Enqueue(nextPosition);
                        PositionSet.Add(nextPosition);
                    }
                }
            }

        }

        while (n > 0 && PositionQueue.Count > 0)
        {
            // 큐에서 위치 꺼내기
            currentPosition = PositionQueue.Dequeue();

            // 방 생성
            GameObject newfloor = Instantiate(floor, currentPosition, Quaternion.identity);
            newfloor.transform.localScale = new Vector3(roomSize, 1, roomSize);
            n--;

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
                        nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z + (10 * roomSize));
                        break;
                    case "Down":
                        nextPosition = new Vector3(currentPosition.x, 0, currentPosition.z - (10 * roomSize));
                        break;
                    case "Left":
                        nextPosition = new Vector3(currentPosition.x - (10 * roomSize), 0, currentPosition.z);
                        break;
                    case "Right":
                        nextPosition = new Vector3(currentPosition.x + (10 * roomSize), 0, currentPosition.z);
                        break;
                }

                // 사용한 방향 제거
                UDLRlist.RemoveAt(index);

                // 접근한적 없는 위치인 경우
                if (!PositionSet.Contains(nextPosition))
                {
                    if (Random.value < 0.5f)
                    {
                        PositionQueue.Enqueue(nextPosition);
                        PositionSet.Add(nextPosition);
                    }
                }
            }
        }
    }
    
    void WallMaker()
    {
        //(0, 0)을 기준으로 상하좌우 좌표에 접근하여 
    }
}
