using UnityEngine;
public class Debuff_AdditionalDamageEffect : IHitEffect
{
    private float rate;
    private float duration;
    public Debuff_AdditionalDamageEffect(float _rate, float _duration)
    {
        this.rate = _rate;
        this.duration = _duration;
    }

    public bool CanApply(HitContext ctx) => true;

    public void Apply(HitContext ctx)
    {
        if (ctx.target.TryGetComponent<NormalMonster>(out var monster))
        {
            monster.TakeDebuff_AdditionalDamage(rate, duration);

        }
        else if (ctx.target.TryGetComponent<BossMonster>(out var boss))
        {
            boss.TakeDebuff_AdditionalDamage(rate, duration);
        }
    }
}
