using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    private bool playerHasEntered = false;

    public void Initialize(float roomDimension)
    {
        BoxCollider triggerCollider = gameObject.AddComponent<BoxCollider>();

        // 2. 'Is Trigger'로 설정 (필수)
        triggerCollider.isTrigger = true;

        // 3. 콜라이더 크기 설정
        // MapMaker에서 방과 방 사이의 거리가 (10 * floorSize)이므로,
        // 트리거의 크기도 이에 맞게 설정하여 방 전체를 덮도록 합니다.
        float size = roomDimension;
        float height = 20f; 
        
        triggerCollider.size = new Vector3(size, height, size);
        
        // 피봇이 바닥(y=0)이므로, 콜라이더의 중심을 y축으로 높이의 절반만큼 올림
        triggerCollider.center = new Vector3(0, height / 2, 0);
    }

    void OnTriggerEnter(Collider other)
    {
        // 이미 한 번 들어왔다면 무시 (중복 실행 방지)
        if (playerHasEntered || !other.CompareTag("Player"))
        {
            return;
        }

        playerHasEntered = true;
        Debug.Log($"플레이어가 {gameObject.name} 방에 입장!");
    }

    // 플레이어가 방을 나갔을 때
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {

        }
    }
}