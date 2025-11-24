using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Deck/ActiveSkillDeck")]
public class ActiveSkillDeckSO : ScriptableObject
{
    [Header("초기 카드 풀 (디자인용, 인스펙터에서 세팅)")]
    [SerializeField] private List<ActiveSkillBase> initialCards = new();

    // 🔹 런타임에서만 사용하는 작업용 리스트 (씬/플레이마다 리셋)
    private List<ActiveSkillBase> runtimeCards;

    private void OnEnable()
    {
        // 에디터에서 도중에 리컴파일될 때도 깔끔하게 초기화되도록
        ResetRuntimeFromInitial();
    }

    /// <summary>
    /// initialCards를 기준으로 runtimeCards를 다시 구성.
    /// - "전체 스킬 덱" 용도에서 사용.
    /// - "획득한 스킬 덱"에서는 호출하지 않고 ClearRuntime/추가만 사용.
    /// </summary>
    public void ResetRuntimeFromInitial()
    {
        if (runtimeCards == null)
            runtimeCards = new List<ActiveSkillBase>();
        else
            runtimeCards.Clear();

        runtimeCards.AddRange(initialCards);
    }

    /// <summary>
    /// 런타임 덱 비우기 (획득 스킬 덱 초기화용).
    /// </summary>
    public void ClearRuntime()
    {
        if (runtimeCards == null)
            runtimeCards = new List<ActiveSkillBase>();
        else
            runtimeCards.Clear();
    }

    /// <summary>
    /// 비복원 추출: runtimeCards에서 하나 뽑고 제거.
    /// (주로 "전체 스킬 덱"에서 사용)
    /// </summary>
    public ActiveSkillBase DrawWithoutReplacementFromRuntime()
    {
        if (runtimeCards == null || runtimeCards.Count == 0)
            return null;

        int index = Random.Range(0, runtimeCards.Count);
        ActiveSkillBase picked = runtimeCards[index];
        runtimeCards.RemoveAt(index);
        return picked;
    }

    /// <summary>
    /// 복원 추출: runtimeCards에서 랜덤 하나 뽑기 (제거 X).
    /// (주로 "획득한 스킬 덱"에서 사용)
    /// </summary>
    public ActiveSkillBase DrawWithReplacementFromRuntime()
    {
        if (runtimeCards == null || runtimeCards.Count == 0)
            return null;

        int index = Random.Range(0, runtimeCards.Count);
        return runtimeCards[index];
    }

    /// <summary>
    /// 런타임 덱에 카드 추가 (에셋(initialCards)은 건드리지 않음).
    /// → "획득한 스킬 덱"에서 쓰기 좋음.
    /// </summary>
    public void AddRuntimeCard(ActiveSkillBase card)
    {
        if (card == null) return;

        if (runtimeCards == null)
            runtimeCards = new List<ActiveSkillBase>();

        if (!runtimeCards.Contains(card))
            runtimeCards.Add(card);
    }

    // 필요하면 디자인용 전체 카드에 접근
    public IReadOnlyList<ActiveSkillBase> InitialCards => initialCards;
}