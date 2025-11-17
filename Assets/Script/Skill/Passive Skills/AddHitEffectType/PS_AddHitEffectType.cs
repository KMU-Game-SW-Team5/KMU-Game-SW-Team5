using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Passive Skill/AddHitEffect")]
public class PS_AddHitEffectType : PassiveSkillBase
{
    [Tooltip("이 스킬로 추가되는 적중시 효과 리스트")]
    public List<HitEffectSO> hitEffects;
}
