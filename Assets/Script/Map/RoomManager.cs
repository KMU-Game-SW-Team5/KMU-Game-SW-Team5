using UnityEngine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    // 이 방에 속한 몬스터들의 리스트
    // 현재 아무것도 없음 - 방을 생성할 때 이 리스트에 몬스터들을 넣어주어야 함
    // public List<MonsterAI> monstersInRoom;

    private bool playerHasEntered = false;

    void OnTriggerEnter(Collider other)
    {
        // 이미 한 번 들어왔다면 무시 (중복 실행 방지)
        if (playerHasEntered) return;

        // 트리거에 부딪힌 것이 "Player" 태그를 가졌는지 확인
        if (other.CompareTag("Player"))
        {
            playerHasEntered = true;
            Debug.Log("플레이어가 방에 입장!");
            
            // 이 방의 모든 몬스터에게 추적 명령 
            // foreach (MonsterAI monster in monstersInRoom)
            // {
            //     monster.StartChase(other.transform);
            // }
        }
    }

    // 플레이어가 방을 나갔을 때
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // 여기에 몬스터 멈추는 로직 (StopChase)을 넣을 수 있음
            // 회의 때 논의 필요... 한데 사실 몬스터를 모두 잡아야 나갈 수 있어서 없어도 될 듯
        }
    }
}