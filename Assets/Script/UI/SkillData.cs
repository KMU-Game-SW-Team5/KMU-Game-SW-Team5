using UnityEngine;

[CreateAssetMenu(menuName = "Data/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public int level;
    public float baseCooldown;
    public string description;
}
