using UnityEngine;
// 데미지에 설정된 고정 값을 더하는 패시브 스킬 타입
[CreateAssetMenu(menuName = "Scriptable Object/HitEffects/AddDamage")]
public class AddDamageSO : HitEffectSO
{
    public float bonusDamage = 10f;
    public override IHitEffect CreateEffectInstance()
    {
        return new AddDamageEffect(bonusDamage);
    }
}