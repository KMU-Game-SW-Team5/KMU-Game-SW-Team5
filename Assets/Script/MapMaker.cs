using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    public int n = 10;              // 생성할 방의 개수
    public int floorSize = 1;       // 바닥 크기
    public GameObject floor;        // 바닥 프리팹
    public int wallSize = 1;        // 벽 크기 일단, floor Size와는 통일할 예정이다.
    public GameObject wall;

    private HashSet<Vector3> PositionSet = new HashSet<Vector3>();
    private Queue<Vector3> PositionQueue = new Queue<Vector3>();

    private Vector3 currentPosition = new Vector3(0, 0, 0); // 시작 지점
    private Vector3 nextPosition;   // 새로 생성될 방 위치

    void Start()
    {
        PlaneMaker();
    }

    void PlaneMaker()
    {
        // 시작 지점 초기화
        PositionQueue.Enqueue(currentPosition);
        PositionSet.Add(currentPosition);

        while (n > 0 && PositionQueue.Count > 0)
        {
            // 큐에서 위치 꺼내기
            currentPosition = PositionQueue.Dequeue();

            // 방 생성
            GameObject newfloor = Instantiate(floor, currentPosition, Quaternion.identity);
            newfloor.transform.localScale = new Vector3(floorSize, 1, floorSize);
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
