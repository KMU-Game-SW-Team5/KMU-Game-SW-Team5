using UnityEngine;

public class DotDamageEffect : IHitEffect
{
    private float dps;
    private float duration;
    public DotDamageEffect(float _dps, float _duration)
    {
        this.dps = _dps;
        this.duration = _duration;
    }

    public bool CanApply(HitContext ctx) => true;

    public void Apply(HitContext ctx)
    {

        // 일반 몬스터는 그냥 적용
        if (ctx.target.TryGetComponent<NormalMonster>(out var monster))
        {
            monster.TakeDOT(dps, duration);
        }
        // 보스 몬스터는 데미지 반으로 감소
        else if (ctx.target.TryGetComponent<BossMonster>(out var boss))
        {
            boss.TakeDOT(dps*0.5f, duration);
        }
    }
}
