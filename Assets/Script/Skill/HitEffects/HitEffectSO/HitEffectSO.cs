using UnityEngine;

public abstract class HitEffectSO : ScriptableObject
{
    /// 런타임에서 사용할 IHitEffect 인스턴스를 생성한다.
    /// HitEffectSO는 데이터를 보관하고,
    /// 실제 효과 실행은 IHitEffect 인스턴스가 담당한다.
    public abstract IHitEffect CreateEffectInstance();
}