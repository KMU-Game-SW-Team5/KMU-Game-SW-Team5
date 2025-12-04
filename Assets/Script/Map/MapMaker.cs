using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMaker : MonoBehaviour
{
    
    [Header("방의 개수")]
    public int roomCount = 10;              // 생성할 방의 개수

    [Header("방의 크기(기본 40)")]
    public int roomSize = 1;        // 방 크기

    [Header("맵관련 프리팹")] 
    public GameObject floor;        // 바닥 프리팹
    public GameObject wall;         // 벽 프리팹 
    public GameObject wallDoor;     // 문이 있는 벽 프리팹 
    public GameObject ceiling;      // 천장 프리팹
    // public GameObject boss;      // [변경] RoomManager에서 Resources로 로드하므로 제거됨

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
    private int currentDifficulty = 0; // [추가] 난이도 저장 변수

    private List<RoomManager> allRooms = new List<RoomManager>();

    private Dictionary<Vector3, Transform> roomMap = new Dictionary<Vector3, Transform>();

    public List<RoomManager> AllRooms => allRooms;
    // KillCountUI와 연결(KillCounter 싱글톤 생성 시 알림)
    public static event Action OnRoomsCreated;
    public bool alreadyRoomsCreated = false;

    void Start()
    {
        ApplyDifficulty();
        FloorAndCeilingMaker();
        WallMaker();
        // BossRoomMaker(); // [변경] 더 이상 별도로 보스방을 만들지 않음
        RoomManagerInitailize();

        Debug.Log("맵 배치 완료.");
        alreadyRoomsCreated = true;
        OnRoomsCreated?.Invoke();
        
    }

    void ApplyDifficulty()
    {   
        // SettingService 클래스가 프로젝트에 존재하며 해당 클래스가 정적으로 선언되어 멤버에 접근
        int difficulty = SettingsService.GameDifficulty;
        currentDifficulty = difficulty; // [추가] RoomManager에 넘겨주기 위해 멤버 변수에 저장

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
        allRooms.Clear();
        roomMap.Clear();
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
            newFloor.transform.SetParent(transform, true);

            // 딕셔너리에 방 정보 저장 (WallMaker에서 사용)
            roomMap.Add(nowPosition, newFloor.transform);

            // 생성되는 방별 RoomManager SetUp
            RoomManager roomMgr = newFloor.GetComponent<RoomManager>();
            allRooms.Add(roomMgr);
            
            // [변경] Setup 호출 시 currentDifficulty 전달
            if (nowPosition == Vector3.zero)
            {
                roomMgr.Setup(RoomType.Start, monsterMin, monsterMax, currentDifficulty);
            }
            else
            {
                roomMgr.Setup(RoomType.Normal, monsterMin, monsterMax, currentDifficulty);
            }   

            // 천장 생성
            Vector3 nowCeilingPosition = nowPosition;
            nowCeilingPosition.y = 10 * 20; // 높이 설정 로직 유지
            Quaternion nowCeilingRotation = Quaternion.Euler(180, 0, 0);
            GameObject newCeiling = Instantiate(ceiling, nowCeilingPosition, nowCeilingRotation);
            newCeiling.transform.SetParent(transform, true);

            // 방이 생성된 위치를 집합에 저장
            PositionSet.Add(nowPosition);

            // 상하좌우 방향 리스트
            List<string> UDLRlist = new List<string> { "Up", "Down", "Left", "Right" };

            bool FirstAccess = true;

            for (int i = 0; i < 4; i++)
            {
                // 무작위 인덱스 선택
                int index = UnityEngine.Random.Range(0, UDLRlist.Count);
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
                        if (UnityEngine.Random.value < 0.5f)
                        {
                            roomCount--;
                            PositionQueue.Enqueue(nextPosition);
                            TempSet.Add(nextPosition);
                        }
                    }
                }
            }
        }

        // 마지막에 생성된 방에 보스방 SetUp 설정
        int assignedBossRooms = 0;
        
        for (int i = allRooms.Count - 1; i >= 0; i--)
        {
            // 할당하려는 보스방 개수를 채웠으면 중단
            if (assignedBossRooms >= bossRoomCount) break;

            RoomManager currentRoom = allRooms[i];

            // 시작 방은 보스방이 될 수 없음
            if (currentRoom.type == RoomType.Start) continue;

            // 방 타입을 Boss로 변경 (몬스터 수는 0으로 설정, 난이도 전달)
            currentRoom.Setup(RoomType.Boss, 0, 0, currentDifficulty);
            assignedBossRooms++;
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

            // 현재 방의 Transform 가져오기 (벽의 부모로 쓰기 위함)
            Transform currentRoomTransform = roomMap.ContainsKey(nowPosition) ? roomMap[nowPosition] : transform;

            for (int i = 0; i < 4; i++)
            {
                // 무작위 인덱스 선택
                int index = UnityEngine.Random.Range(0, UDLRlist.Count);
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
                        doorWall.transform.SetParent(currentRoomTransform, true);
                        
                        ToarchMaker(doorWall);
                    }
                    else // TempSet에 있는 위치라면(이미 접근한 적 있는 방이라면)
                    {
                        if (!WallSet.Contains(wallPosition))    // 옆방과 현재 방 사이에 벽이 없는 경우
                        {
                            WallSet.Add(wallPosition);

                            if (UnityEngine.Random.value < 0.2f) // 20% 확률로 벽을 만든다. 
                            {
                                // 문이 있는 벽 생성
                                GameObject doorWall = Instantiate(wallDoor, wallPosition, wallRotation);
                                doorWall.transform.localScale *= roomSize;
                                // 생성 후 부모 설정
                                doorWall.transform.SetParent(currentRoomTransform, true);

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
                                newWall.transform.SetParent(currentRoomTransform, true);

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
                    newWall.transform.SetParent(currentRoomTransform, true);

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

    // void BossRoomMaker() { ... } // 제거됨

    void RoomManagerInitailize()
    {   
        // 모든 맵의 배치가 끝난 뒤의 방의 설정별로 RoomManager 생성
        foreach(var room in allRooms)
        {
            room.Initialize();
        }
    }
}