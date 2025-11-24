using UnityEngine;
// 데미지에 설정된 고정 값을 더하는 패시브 스킬 타입
[CreateAssetMenu(menuName = "Scriptable Object/HitEffects/Dot Damage")]
public class DotDamageSO : HitEffectSO
{
    [Range(0f, 1000f)] public float dps;
    [Range(0f, 30f)] public float duration;
    public override IHitEffect CreateEffectInstance()
    {
        return new DotDamageEffect(dps, duration);
    }
}