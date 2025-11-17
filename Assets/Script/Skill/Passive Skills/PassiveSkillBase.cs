using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveSkillBase : ScriptableObject
{
    [Header("스킬 정보")]
    [SerializeField] private string skillName;            // 스킬 이름
    [SerializeField] private Sprite icon;                 // 스킬 아이콘
    [SerializeField] private string describe;             // 스킬 설명
}
