using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Deck/PassiveSkillDeck")]
public class PassiveSkillDeckSO : ScriptableObject
{
    [Header("이 덱이 사용할 패시브 스킬들 (인스펙터에서 세팅)")]
    [SerializeField] private List<PassiveSkillBase> initialCards = new();

    private List<PassiveSkillBase> workingCards;

    private void OnEnable()
    {
        ResetDeck();
    }

    public void ResetDeck()
    {
        if (workingCards == null)
            workingCards = new List<PassiveSkillBase>();
        else
            workingCards.Clear();

        workingCards.AddRange(initialCards);
    }

    public PassiveSkillBase DrawWithoutReplacement()
    {
        if (workingCards == null || workingCards.Count == 0)
            return null;

        int index = Random.Range(0, workingCards.Count);
        PassiveSkillBase picked = workingCards[index];
        workingCards.RemoveAt(index);
        return picked;
    }

    public PassiveSkillBase DrawWithReplacement()
    {
        if (initialCards == null || initialCards.Count == 0)
            return null;

        int index = Random.Range(0, initialCards.Count);
        return initialCards[index];
    }

    public void AddCard(PassiveSkillBase card, bool addToWorkingDeck = true)
    {
        if (card == null) return;

        if (!initialCards.Contains(card))
            initialCards.Add(card);

        if (addToWorkingDeck)
        {
            if (workingCards == null)
                workingCards = new List<PassiveSkillBase>();
            workingCards.Add(card);
        }
    }

    public IReadOnlyList<PassiveSkillBase> InitialCards => initialCards;
}
