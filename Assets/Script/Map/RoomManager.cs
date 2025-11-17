using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("MapMaker 컴포넌트")]
    public MapMaker MapMaker;

    [Header("몬스터 설정")]
    public GameObject TempMonster;
    public int monstersPerRoom = 1;

    private int roomSize;
    private bool clearFlag = false;
    private bool playerEntered = false; // 스폰 중복 방지

    void Start()
    {
        // BoxCollider 접근
        BoxCollider triggerCollider = GetComponent<BoxCollider>();

        // MapMaker의 방 크기 정보
        roomSize = MapMaker.roomSize;

        // 트리거 범위 설정
        triggerCollider.size = new Vector3(roomSize / 4, 10, roomSize / 4);
        triggerCollider.center = new Vector3(0, 0, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        // 플레이어 아니면 무시
        if (!other.CompareTag("Player"))
            return;

        // 이미 스폰했거나 클리어된 방이면 무시
        if (playerEntered || clearFlag)
            return;

        // 몬스터 스폰
        for (int i = 0; i < monstersPerRoom; i++)
        {
            float spawnRadius = (roomSize * 10 / 2) * 0.8f;

            Vector3 spawnPos = transform.position + new Vector3(
                Random.Range(-spawnRadius, spawnRadius),
                10f, // 공중이 아니라 적절한 높이
                Random.Range(-spawnRadius, spawnRadius)
            );

            // 몬스터 생성
            GameObject newMonster = Instantiate(TempMonster, spawnPos, Quaternion.identity);
            newMonster.name = $"Monster_{gameObject.name}_{i + 1}";

            // 방을 부모로 설정
            // newMonster.transform.SetParent(transform);
            newMonster.transform.localScale = new Vector3(roomSize/20, roomSize/20, roomSize/20);
        }

        playerEntered = true;
    }

    // 방에 몬스터가 죽을때마 호출되서 몬스터가 모두 죽었는지 확인 
    public void MonsterKilled()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Monster"))
                return; // 몬스터가 하나라도 남음
        }

        clearFlag = true;
        Debug.Log($"{gameObject.name} 방 클리어!");
    }
}
