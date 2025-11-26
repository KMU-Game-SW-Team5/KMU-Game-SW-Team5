using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;

public class MapMaker : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    [Header("방의 개수")]
    public int roomCount = 10;              // 생성할 방의 개수

    [Header("방의 크기(기본 40)")]
    public int roomSize = 1;        // 방 크기

    [Header("맵관련 프리팹")] 
    public GameObject floor;        // 바닥 프리팹
    public GameObject wall;         // 벽 프리팹 
    public GameObject wallDoor;     // 문이 있는 벽 프리팹 
    public GameObject ceiling;      // 천장 프리팹
    public GameObject boss;         // 보스몹 프리팹

    [Header("몬스터 수")]
    public int monsterMin = 15;
    public int monsterMax = 15;

    private HashSet<Vector3> TempSet = new HashSet<Vector3>();
    private HashSet<Vector3> PositionSet = new HashSet<Vector3>();
    private HashSet<Vector3> WallSet = new HashSet<Vector3>();
    private Queue<Vector3> PositionQueue = new Queue<Vector3>();
    private Vector3 nextPosition;   // 새로 생성될 방 위치
    private Vector3 bossPosition;
    private int bossRoomCount;

    // mapHolder 변수 제거됨 (this.transform 사용)

    void Start()
    {
        ApplyDifficulty();
        FloorAndCeilingMaker();
        WallMaker();
        
        // 여기에 장애물/모듈 랜덤 배치 함수가 있다면 호출
        // PlaceRandomModules(); 

        BossRoomMaker();

        Debug.Log("맵 배치 완료.");

        // [핵심] 맵과 장애물이 다 깔린 뒤에 네비게이션 굽기 실행
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh 굽기 완료 (파란색 지도가 생성됨)");
        }
        else
        {
            Debug.LogError("NavMeshSurface가 연결되지 않았습니다!");
        }
    }

    void ApplyDifficulty()
    {   
        // SettingService 클래스가 프로젝트에 존재하며 해당 클래스가 정적으로 선언되어 멤버에 접근
        int difficulty = SettingsService.GameDifficulty;

        switch (difficulty)
        {
            case 0: // easy 
                monsterMin -= 5;
                monsterMax -= 5;
                bossRoomCount = 1;
                break;
            case 1: // Normal (보통)
                monsterMin += 0;
                monsterMax += 0;
                bossRoomCount = 2;
                break;
            case 2: // Hard (어려움)
                monsterMin += 5;
                monsterMax += 5;
                bossRoomCount = 3;
                break;
        }   
    }

    void FloorAndCeilingMaker()
    {   
        // 큐와 집합 초기화 및 초기값 설정
        PositionQueue.Clear();
        TempSet.Clear();
        PositionQueue.Enqueue(new Vector3(0, 0, 0));
        TempSet.Add(new Vector3(0, 0, 0));
        roomCount--;

        while (PositionQueue.Count != 0)
        {
            // 큐에서 위치 꺼내기
            Vector3 nowPosition = PositionQueue.Dequeue();
            
            bossPosition = nowPosition;

            // 방생성
            GameObject newFloor = Instantiate(floor, nowPosition, Quaternion.identity);
            newFloor.transform.localScale = new Vector3(roomSize, 1, roomSize);
            // 생성 후 부모 설정 (this.transform 사용)
            newFloor.transform.SetParent(transform, true);

            // ★ 추가된 부분: RoomManager 설정 ★
            RoomManager roomMgr = newFloor.GetComponent<RoomManager>();
            if (PositionQueue.Count + 1 <= bossRoomCount)   // 현재 방이 보스방이 생성되어야할 방일때 
            {
                roomMgr.Setup(RoomType.Boss, 0, 0);
            }
            else
            {
                // (0,0,0) 위치면 시작 방, 아니면 일반 방으로 설정
                if (nowPosition == Vector3.zero)
                {
                    roomMgr.Setup(RoomType.Start, monsterMin, monsterMax);
                }
                else
                {
                    roomMgr.Setup(RoomType.Normal, monsterMin, monsterMax);
                }   
            }

            // 천장 생성
            Vector3 nowCeilingPosition = nowPosition;
            nowCeilingPosition.y = 10 * 20; // 높이 설정 로직 유지
            Quaternion nowCeilingRotation = Quaternion.Euler(180, 0, 0);
            GameObject newCeiling = Instantiate(ceiling, nowCeilingPosition, nowCeilingRotation);
            // newCeiling.transform.localScale = new Vector3(roomSize, 1, roomSize);
            // 생성 후 부모 설정
            newCeiling.transform.SetParent(transform, true);

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
                if (roomCount != 0 && !TempSet.Contains(nextPosition))
                {
                    if (FirstAccess)
                    {
                        roomCount--;
                        PositionQueue.Enqueue(nextPosition);
                        TempSet.Add(nextPosition);
                        FirstAccess = false;
                    }
                    else
                    {
                        if (Random.value < 0.5f)
                        {
                            roomCount--;
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
                        // 생성 후 부모 설정
                        doorWall.transform.SetParent(transform, true);
                        
                        ToarchMaker(doorWall);
                    }
                    else // TempSet에 있는 위치라면(이미 접근한 적 있는 방이라면)
                    {
                        if (!WallSet.Contains(wallPosition))    // 옆방과 현재 방 사이에 벽이 없는 경우
                        {
                            WallSet.Add(wallPosition);

                            if (Random.value < 0.2f) // 20% 확률로 벽을 만든다. 
                            {
                                // 문이 있는 벽 생성
                                GameObject doorWall = Instantiate(wallDoor, wallPosition, wallRotation);
                                doorWall.transform.localScale *= roomSize;
                                // 생성 후 부모 설정
                                doorWall.transform.SetParent(transform, true);

                                ToarchMaker(doorWall);
                            }
                            else
                            {
                                // 문이 없는 벽 생성, 및 방 크기 변경
                                Vector3 adjustedWallPos = wallPosition;
                                adjustedWallPos.y += wall.transform.localScale.y * ((roomSize / 2));
                                GameObject newWall = Instantiate(wall, adjustedWallPos, wallRotation);
                                newWall.transform.localScale *= roomSize;
                                // 생성 후 부모 설정
                                newWall.transform.SetParent(transform, true);

                                ToarchMaker(newWall);

                            }
                        }
                    }
                }
                else // 방을 생성할 수 있는 위치가 아니라면 (외벽)
                {
                    // 문이 없는 벽 생성, 및 방 크기 변경
                    Vector3 adjustedWallPos = wallPosition;
                    adjustedWallPos.y += wall.transform.localScale.y * ((roomSize / 2));
                    GameObject newWall = Instantiate(wall, adjustedWallPos, wallRotation);
                    newWall.transform.localScale *= roomSize;
                    // 생성 후 부모 설정
                    newWall.transform.SetParent(transform, true);

                    ToarchMaker(newWall);
                }

            }
        }
    }

    void ToarchMaker(GameObject gameObject)
    {
        // ✅ 모든 Light 조정
        Light[] lights = gameObject.GetComponentsInChildren<Light>();
        foreach (var light in lights)
        {
            // Range (조명 거리) 확대
            light.range *= roomSize;
        }

        // ✅ 모든 Particle System 조정
        ParticleSystem[] particles = gameObject.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            // 입자 크기와 속도 비례 확대
            var main = ps.main;
            main.startSizeMultiplier *= roomSize;
            main.startSpeedMultiplier *= roomSize;
        }
    }

    void BossRoomMaker()
    {
        bossPosition.y += 20;
        GameObject newfloor = Instantiate(boss, bossPosition, Quaternion.identity);

        BossMonsterBase bossController = newfloor.GetComponent<BossMonsterBase>();
        BossManager.Instance.RegisterBoss(bossController);
    }
}