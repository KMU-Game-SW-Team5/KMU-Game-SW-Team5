using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Deck/ActiveSkillDeck")]
public class ActiveSkillDeckSO : ScriptableObject
{
    [Header("초기 카드 풀 (디자인용, 인스펙터에서 세팅)")]
    [SerializeField] private List<ActiveSkillBase> initialCards = new();
    public IReadOnlyList<ActiveSkillBase> InitialCards => initialCards;

    // 🔹 런타임에서만 사용하는 작업용 리스트 (씬/플레이마다 리셋)
    [SerializeField]
    private List<ActiveSkillBase> runtimeCards = new();

    // 🔹 런타임 덱에 들어있는 카드 개수 (중복 액티브 덱에서 사용)
    public int RuntimeCount => runtimeCards?.Count ?? 0;

    private void OnEnable()
    {
        ResetRuntimeFromInitial();
    }

    public void ResetRuntimeFromInitial()
    {
        if (runtimeCards == null)
            runtimeCards = new List<ActiveSkillBase>();
        else
            runtimeCards.Clear();

        runtimeCards.AddRange(initialCards);
    }

    public void ClearRuntime()
    {
        if (runtimeCards == null)
            runtimeCards = new List<ActiveSkillBase>();
        else
            runtimeCards.Clear();
    }

    /// <summary>
    /// ✅ 덱에서 랜덤 카드 하나 "보기만" 한다. (제거 X)
    /// </summary>
    public ActiveSkillBase GetRandomFromRuntime()
    {
        if (runtimeCards == null || runtimeCards.Count == 0)
            return null;

        int index = Random.Range(0, runtimeCards.Count);
        return runtimeCards[index];
    }

    /// <summary>
    /// ✅ 특정 카드를 런타임 덱에서 제거 (확정 선택 시 호출)
    /// </summary>
    public void RemoveRuntimeCard(ActiveSkillBase card)
    {
        if (runtimeCards == null || card == null) return;
        runtimeCards.Remove(card);
    }

    /// <summary>
    /// (기존 함수) 비복원 추출 – 다른 데서 쓰고 있으면 그대로 둬도 됨
    /// </summary>
    public ActiveSkillBase DrawWithoutReplacementFromRuntime()
    {
        if (runtimeCards == null || runtimeCards.Count == 0)
            return null;

        int index = Random.Range(0, runtimeCards.Count);
        var picked = runtimeCards[index];
        runtimeCards.RemoveAt(index);
        return picked;
    }

    public ActiveSkillBase DrawWithReplacementFromRuntime()
    {
        if (runtimeCards == null || runtimeCards.Count == 0)
            return null;

        int index = Random.Range(0, runtimeCards.Count);
        return runtimeCards[index];
    }

    public void AddRuntimeCard(ActiveSkillBase card)
    {
        if (card == null)
        {
            Debug.Log("add card is null");
            return;
        }

        if (runtimeCards == null)
            runtimeCards = new List<ActiveSkillBase>();

        if (!runtimeCards.Contains(card))
            runtimeCards.Add(card);
    }

}
