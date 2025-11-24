using UnityEngine;
// 데미지에 설정된 고정 값을 더하는 패시브 스킬 타입
[CreateAssetMenu(menuName = "Scriptable Object/HitEffects/stun")]
public class StunSO : HitEffectSO
{
    [Range(0f, 10f)] public float stunDuration;
    public override IHitEffect CreateEffectInstance()
    {
        return new StunEffect(stunDuration);
    }
}