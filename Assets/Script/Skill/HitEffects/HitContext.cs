using UnityEngine;
// 적중 시 전달되는 정보들을 정의한 구조체
public struct HitContext
{
    public GameObject attacker;     // 시전자(플레이어)
    public GameObject target;       // 적중한 대상
    public Vector3 hitPoint;        // 적중한 위치
    public float baseDamage;        // 패시브 스킬이 적용되지 않은 스킬 자체의 데미지
    public Object source;           // 적중한 스킬의 종류 (투사체형 등)

    public HitContext(
        GameObject attacker,
        GameObject target,
        Vector3 hitPoint,
        float baseDamage,
        Object source)
    {
        this.attacker = attacker;
        this.target = target;
        this.hitPoint = hitPoint;
        this.baseDamage = baseDamage;
        this.source = source;
    }
}
