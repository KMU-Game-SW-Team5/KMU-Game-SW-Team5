using UnityEngine;

public class NormalMonster : MonsterBase
{
    private Transform player;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected override void Update()
    {
        base.Update();
        //if (IsAlive && player != null)
        //    MoveTowards(player.position);
    }

    protected override void OnHit(GameObject attacker)
    {
        // 맞았을 때 반응 (넉백, 애니메이션 등)
    }

    protected override void OnDeath(GameObject killer)
    {
        // 사망시 드랍 아이템 처리 등
    }
}
