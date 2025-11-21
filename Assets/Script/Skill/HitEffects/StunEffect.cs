using UnityEngine;
public class StunEffect : IHitEffect
{
    private float stunDuration;
    public StunEffect(float _stunDuration)
    {
        this.stunDuration = _stunDuration;
    }

    public bool CanApply(HitContext ctx) => true;

    public void Apply(HitContext ctx)
    {
        // 일반 몬스터의 경우 기절 그대로 적용
        if (ctx.target.TryGetComponent<NormalMonster>(out var monster))
        {
            monster.TakeStun(stunDuration);
        }

        // 보스 몬스터의 경우 지속시간 절반
        else if (ctx.target.TryGetComponent<BossMonster>(out var boss))
        {
            boss.TakeStun(stunDuration * 0.5f);
        }
    }
}
