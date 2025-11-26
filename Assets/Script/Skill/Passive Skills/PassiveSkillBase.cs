using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PassiveSkillBase : ScriptableObject
{
    [Header("스킬 정보")]
    [SerializeField] private string skillName;            // 스킬 이름
    [SerializeField] private Sprite icon;                 // 스킬 아이콘
    [SerializeField] private string description;             // 스킬 설명

    // 스킬 정보의 getter들
    public Sprite GetIcon() => icon;
    public string GetSkillName() => skillName;
    public string GetSkillDescription() => description;

    public override string ToString()
    {
        return skillName;
    }
}
