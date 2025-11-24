using UnityEngine;
// 데미지에 설정된 고정 값을 더하는 패시브 스킬 타입
[CreateAssetMenu(menuName = "Scriptable Object/HitEffects/slow")]
public class SlowSO : HitEffectSO
{
    [Range(0f, 1f)] public float slowRate;
    [Range(0f, 10f)] public float slowDuration;
    public override IHitEffect CreateEffectInstance()
    {
        return new SlowEffect(slowRate, slowDuration);
    }
}