using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MinimapUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform content; // 방 아이콘들이 들어갈 부모
    [SerializeField] private RectTransform playerPoint; // 플레이어 위치 표시 오브젝트
    [SerializeField] private RectTransform roomPrefab;  // 방 한 칸 아이콘 프리팹
    [SerializeField] private Sprite normalRoomSprite;   
    [SerializeField] private Sprite bossRoomSprite;  

    [Header("Layout")]
    [SerializeField] private float cellSize = 40f;  // 방 아이콘 크기
    [SerializeField] private float gap = 10f;   // 방 사이 간격

    private readonly Dictionary<Vector2Int, RectTransform> roomIcons =
        new Dictionary<Vector2Int, RectTransform>();

    private Vector2 centerOffset;

    readonly Color NotClearNotVisitColor = new Color(0.35f, 0.35f, 0.35f, 1f);
    readonly Color VisitedColor = new Color(1f, 1f, 1f, 1f);

    public static readonly List<MinimapUI> Instances = new List<MinimapUI>();

    private void Awake()
    {
        Instances.Add(this);
    }

    private void OnDestroy()
    {
        Instances.Remove(this);
    }

    public void Build(List<MinimapRoomData> rooms, MinimapRoomData startRoom)
    {
        // 기존 아이콘 제거
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }
        roomIcons.Clear();

        if (rooms == null || rooms.Count == 0)
            return;

        // 그리드 좌표 범위 계산 (전체를 중앙에 모으기 위함)
        int minX = rooms.Min(r => r.gridPos.x);
        int maxX = rooms.Max(r => r.gridPos.x);
        int minY = rooms.Min(r => r.gridPos.y);
        int maxY = rooms.Max(r => r.gridPos.y);

        int widthCount = maxX - minX;   // 칸 수 - 1
        int heightCount = maxY - minY;

        float width = widthCount * (cellSize + gap);
        float height = heightCount * (cellSize + gap);

        // content 중심을 기준으로 전체 방 묶음이 가운데 오도록 오프셋 계산
        centerOffset = new Vector2(-width * 0.5f, -height * 0.5f);

        // 각 방 아이콘 생성
        foreach (var room in rooms)
        {
            CreateRoomIcon(room, minX, minY);
        }

        // 시작 방 강조
        if (startRoom != null)
        {
            SetCurrentRoom(startRoom.gridPos);
        }
    }

    private void CreateRoomIcon(MinimapRoomData roomData, int minX, int minY)
    {
        RectTransform icon = Instantiate(roomPrefab, content);
        icon.gameObject.SetActive(true);

        // 해당 프리팹이 자동으로 content의 로컬 좌표를 기준으로 배치된다.
        Vector2 anchoredPos = GridToUI(roomData.gridPos, minX, minY);
        icon.anchoredPosition = anchoredPos;

        Image img = icon.GetComponent<Image>();
        if (img != null)
        {
            img.color = NotClearNotVisitColor;
            switch (roomData.type)
            {
                case RoomType.Boss:
                    img.sprite = bossRoomSprite;
                    break;
                default:
                    img.sprite = normalRoomSprite;
                    break;
            }
        }

        roomIcons[roomData.gridPos] = icon;
    }

    private Vector2 GridToUI(Vector2Int gridPos, int minX, int minY)
    {
        // 최소 좌표를 기준으로 0부터 시작하는 인덱스로 변환
        int ix = gridPos.x - minX;
        int iy = gridPos.y - minY;

        float x = ix * (cellSize + gap);
        float y = iy * (cellSize + gap);

        return new Vector2(x, y) + centerOffset;
    }

    /// <summary>
    /// 플레이어의 Y 회전값을 받아서 미니맵을 회전시킨다.
    /// playerYaw = playerTransform.eulerAngles.y
    /// </summary>
    public void UpdateRotation(float playerYaw)
    {
        // 플레이어가 항상 위방향을 보도록
        content.parent.localRotation = Quaternion.Euler(0f, 0f, playerYaw);
        playerPoint.localRotation = Quaternion.Euler(0f, 0f, -playerYaw);
    }

    /// <summary>
    /// 현재 방을 바꾸고 싶을 때 호출 (플레이어 이동 시)
    /// </summary>
    public void SetCurrentRoom(Vector2Int gridPos)
    {
        // 전체 아이콘 리셋
        foreach (var kvp in roomIcons)
        {
            RectTransform icon = kvp.Value;
            icon.localScale = Vector3.one;
        }

        // 현재 방만 살짝 키우기
        if (roomIcons.TryGetValue(gridPos, out RectTransform current))
        {
            current.localScale = Vector3.one * 1.2f;

            Image currentImg = current.GetComponent<Image>();
            currentImg.color = VisitedColor;

            Vector2 targetCenter = Vector2.zero;
            Vector2 iconPos = current.anchoredPosition;
            Vector2 offset = targetCenter - iconPos;

            content.anchoredPosition = offset;
        }
    }
}
