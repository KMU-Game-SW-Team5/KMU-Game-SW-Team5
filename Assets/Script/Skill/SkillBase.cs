using UnityEngine;

public abstract class SkillBase : ScriptableObject
{
    public string skillName;    // 스킬 이름
    public Sprite icon;         // 스킬 아이콘
    public string description;  // 스킬 설명

    // 습득 시 호출
    public virtual void OnEquip(GameObject user) { }
    // 제거 시 호출
    public virtual void OnUnequip(GameObject user) { }
}