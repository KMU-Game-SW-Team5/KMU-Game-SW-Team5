using UnityEngine;
// 데미지에 설정된 고정 값을 더하는 패시브 스킬 타입
[CreateAssetMenu(menuName = "Scriptable Object/HitEffects/Debuff_AdditionalDamage")]
public class Debuff_AdditionalDamageSO : HitEffectSO
{
    [Range(0f, 10f)] public float rate;
    [Range(0f, 10f)] public float duration;
    public override IHitEffect CreateEffectInstance()
    {
        return new Debuff_AdditionalDamageEffect(rate, duration);
    }
}