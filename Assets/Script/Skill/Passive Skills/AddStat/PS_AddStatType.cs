using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Passive Skill/Add stat")]
public class PS_AddStatType : PassiveSkillBase
{
    [Tooltip("증가하는 스탯 종류와 값")]
    public BuffStatType buffStatType;
    public float amount;
}