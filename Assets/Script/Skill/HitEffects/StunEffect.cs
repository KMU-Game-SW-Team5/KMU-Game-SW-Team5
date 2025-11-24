using UnityEngine;

public class StunEffect : IHitEffect
{
    private float stunDuration;

    public StunEffect(float _stunDuration)
    {
        stunDuration = _stunDuration;
    }

    public bool CanApply(HitContext ctx) => true;

    public void Apply(HitContext ctx)
    {
        if (ctx.target.TryGetComponent<MonsterBase>(out var monster))
        {
            monster.TakeStun(stunDuration);
        }
    }
}
