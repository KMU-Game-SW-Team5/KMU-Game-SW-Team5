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


    [Header("중복 획득 설정")]
    [Tooltip("이 패시브 스킬을 최대 몇 번까지 획득할 수 있는지. 0 이하이면 무제한으로 간주.")]
    [SerializeField] private int maxAcquireCount = 0;

    [Header("수치 정보 (선택)")]
    [Tooltip("한 번 획득할 때마다 증가하는 값 (예: 최대 체력 +10, 이동속도 +5 등)")]
    [SerializeField] private float valuePerStack = 0f;

    [Tooltip("누적 수치를 표시할 때 사용할 템플릿. {value}와 {count}를 치환해서 사용.\n예: \"최대 체력 {value} 증가 (획득 {count}회)\"")]
    [TextArea]
    [SerializeField]
    private string stackedDescriptionTemplate = "Get stat by {value}  (x{count})";

    public string StackedDescriptionTemplate => stackedDescriptionTemplate;


    public int MaxAcquireCount => maxAcquireCount;
    public float ValuePerStack => valuePerStack;


    /// stackCount에 따른 누적 수치를 포함한 설명 문자열을 반환.
    /// valuePerStack이 0이면 그냥 기본 description에 (xN)만 붙이도록 처리.
    public string GetStackedDescription(int stackCount)
    {
        // 수치 정보가 없으면, 그냥 기본 설명 + (xN) 정도만 붙여주기
        if (Mathf.Approximately(valuePerStack, 0f))
        {
            if (stackCount <= 1)
                return description;
            else
                return $"{description} (X {stackCount})";
        }

        // 수치 정보가 있다면 1회일 때도 포함해서 항상 total 계산
        int safeCount = Mathf.Max(1, stackCount);
        float total = valuePerStack * safeCount;

        Debug.Log(safeCount.ToString() + " | " + total.ToString());

        string text = stackedDescriptionTemplate;
        text = text.Replace("{value}", total.ToString());
        text = text.Replace("{count}", safeCount.ToString());

        return text;
    }



    public override string ToString()
    {
        return skillName;
    }
}
