using UnityEngine;

public class AddDamageEffect : IHitEffect
{
    private float bonusDamage;

    public AddDamageEffect(float bonusDamage)
    {
        this.bonusDamage = bonusDamage;
    }

    public bool CanApply(HitContext ctx) => true;

    public void Apply(HitContext ctx)
    {

        // MonsterBase 기반 몬스터인지 확인
        if (ctx.target.TryGetComponent<NormalMonster>(out var monster))
        {
            monster.TakeDamage(bonusDamage, ctx.attacker);
        }
    }
}
