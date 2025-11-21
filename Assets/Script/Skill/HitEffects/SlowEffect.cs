using UnityEngine;

public class SlowEffect : IHitEffect
{
    [Range(0f, 1f)] private float slowRate;
    [Range(0f, 10f)] private float slowDuration;
    public SlowEffect(float _slowRate, float _slowDuration)
    {
        this.slowRate = _slowRate;
        this.slowDuration = _slowDuration;
    }

    public bool CanApply(HitContext ctx) => true;

    public void Apply(HitContext ctx)
    {

        // MonsterBase 기반 몬스터인지 확인
        if (ctx.target.TryGetComponent<NormalMonster>(out var monster))
        {
            monster.TakeSlow(1 - slowRate, slowDuration);
        }
    }
}
