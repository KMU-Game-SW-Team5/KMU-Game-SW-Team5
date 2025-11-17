public interface IHitEffect
{
    /// 적중 조건에 따라 발동 가능한지 판단.
    /// 예: 근접 공격에서만, 보스에게만, 중독 상태에서만 등.
    bool CanApply(HitContext ctx);

    /// 적중 효과 적용.
    void Apply(HitContext ctx);
}