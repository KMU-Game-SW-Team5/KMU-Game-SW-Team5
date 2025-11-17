using UnityEngine;

public class BossMonster : MonsterBase
{
    private Animator anim;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponentInChildren<Animator>();
    }

    protected override void OnHit(GameObject attacker)
    {
        anim.SetTrigger("Hit");
    }

    protected override void OnDeath(GameObject killer)
    {
        anim.SetTrigger("Die");
        Destroy(gameObject, 3f);
    }
}
