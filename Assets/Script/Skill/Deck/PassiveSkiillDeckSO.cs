using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Deck/PassiveSkillDeck")]
public class PassiveSkillDeckSO : ScriptableObject
{
    [Header("이 덱이 사용할 패시브 스킬들 (인스펙터에서 세팅)")]
    [SerializeField] private List<PassiveSkillBase> initialCards = new();

    [Header("실제 뽑기에 쓰이는 덱 상태 (디버그용)")]
    [SerializeField] private List<PassiveSkillBase> workingCards = new();  // 항상 new 로 시작

    // 🔹 아직 드로우 가능한 패시브 카드 종류 수
    // (MaxAcquireCount 다 찬 카드는 RemoveCard에서 initialCards/workingCards에서 빠짐)
    public int DrawableCount => workingCards?.Count ?? 0;

    public IReadOnlyList<PassiveSkillBase> InitialCards => initialCards;
    public IReadOnlyList<PassiveSkillBase> WorkingCards => workingCards;

    private void OnEnable()
    {
        ResetDeck();
        DebugPrintDeck("[OnEnable] 덱 초기화");
    }

    public void ResetDeck()
    {
        if (workingCards == null)
            workingCards = new List<PassiveSkillBase>();
        else
            workingCards.Clear();

        workingCards.AddRange(initialCards);
        DebugPrintDeck("[ResetDeck] 덱 리셋");
    }

    public PassiveSkillBase DrawWithoutReplacement()
    {
        if (workingCards == null || workingCards.Count == 0)
        {
            Debug.LogWarning("[PassiveSkillDeckSO] DrawWithoutReplacement 호출했지만 workingCards 가 null 또는 비어있음", this);
            DebugPrintDeck("[DrawWithoutReplacement] 실패 상태");
            return null;
        }

        int index = Random.Range(0, workingCards.Count);
        PassiveSkillBase picked = workingCards[index];
        workingCards.RemoveAt(index);

        DebugPrintDeck($"[DrawWithoutReplacement] '{picked?.name}' 뽑음");
        return picked;
    }

    // 수정: 교체 뽑기(복원 가능)도 workingCards에서 뽑도록 변경.
    // 이렇게 하면 이미 덱에서 제거된(최대 획득 도달) 카드는 더 이상 뽑히지 않습니다.
    public PassiveSkillBase DrawWithReplacement()
    {
        if (workingCards == null || workingCards.Count == 0)
        {
            Debug.LogWarning("[PassiveSkillDeckSO] DrawWithReplacement 호출했지만 workingCards 가 null 또는 비어있음", this);
            DebugPrintDeck("[DrawWithReplacement] 실패 상태");
            return null;
        }

        int index = Random.Range(0, workingCards.Count);
        PassiveSkillBase picked = workingCards[index];

        Debug.Log($"[PassiveSkillDeckSO] [DrawWithReplacement from workingCards] '{picked?.name}' 뽑음", this);
        return picked;
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

        DebugPrintDeck($"[AddCard] '{card.name}' 추가 (working에 추가 여부: {addToWorkingDeck})");
    }

    // 덱에서 카드 제거
    public void RemoveCard(PassiveSkillBase card)
    {
        if (card == null) return;

        
        if (workingCards != null)
        {
            // 여러 번 들어 있을 수도 있으니 전부 제거
            while (workingCards.Remove(card)) { }
        }

        DebugPrintDeck($"[RemoveCard] '{card.name}' 제거");
    }


    // =========================
    // 디버그용 유틸 함수들
    // =========================

    [ContextMenu("디버그/현재 덱 상태 출력")]
    private void ContextMenuPrintDeck()
    {
        DebugPrintDeck("[ContextMenu] 수동으로 덱 상태 출력");
    }

    private void DebugPrintDeck(string prefix)
    {
        string initialList = initialCards == null || initialCards.Count == 0
            ? "(비어있음)"
            : string.Join(", ", initialCards.ConvertAll(c => c ? c.name : "null"));

        string workingList = workingCards == null || workingCards.Count == 0
            ? "(비어있음)"
            : string.Join(", ", workingCards.ConvertAll(c => c ? c.name : "null"));

        Debug.Log(
            $"{prefix}\n" +
            $"  InitialCards ({initialCards?.Count ?? 0}개): {initialList}\n" +
            $"  WorkingCards ({workingCards?.Count ?? 0}개): {workingList}",
            this
        );
    }
}
