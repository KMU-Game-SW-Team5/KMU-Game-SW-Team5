using System.Collections.Generic;
using UnityEngine;

public class MinimapBuilder : MonoBehaviour
{
    [SerializeField] MapMaker mapMaker;
    [SerializeField] MinimapUI minimapUI; // UI 기반 미니맵 스크립트

    private void OnEnable()
    {
        if (mapMaker != null && mapMaker.alreadyRoomsCreated)
        {
            CreateMinimap();
        }
        else
        {
            MapMaker.OnRoomsCreated += CreateMinimap;
        }
    }

    private void OnDisable()
    {
        MapMaker.OnRoomsCreated -= CreateMinimap;
    }

    void CreateMinimap()
    {
        List<MinimapRoomData> roomDatas = new List<MinimapRoomData>();

        foreach (var roomMgr in mapMaker.AllRooms)
        {
            Vector3 worldPos = roomMgr.transform.position;

            int gx = Mathf.RoundToInt(worldPos.x / (10 * mapMaker.roomSize));
            int gy = Mathf.RoundToInt(worldPos.z / (10 * mapMaker.roomSize));

            roomDatas.Add(new MinimapRoomData
            {
                gridPos = new Vector2Int(gx, gy),
                type = roomMgr.type,
                roomManager = roomMgr
            });
        }

        // 스타트룸 찾기
        MinimapRoomData startRoom = roomDatas.Find(r => r.type == RoomType.Start);

        // UI에 넘겨주기
        foreach (var ui in MinimapUI.Instances)
        {
            ui.Build(roomDatas, startRoom);
        }
    }
}

public class MinimapRoomData
{
    public Vector2Int gridPos;
    public RoomType type;
    public RoomManager roomManager;
}
