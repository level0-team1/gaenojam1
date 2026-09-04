using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private const int MAX_SLOTS = 5;

    [SerializeField] private List<IngredientSO> ownedCards = new List<IngredientSO>();

    [Header("상태 플래그")]
    public bool isReplacing = false;
    public bool isUIOpen    = false;
    public IngredientSO pendingCard { get; private set; }

    [Header("썩은 재료 (곰팡이 카드용)")]
    [SerializeField] private IngredientSO rottenIngredient;

    public SpecialCardSO heldSpecialCard { get; private set; }

    public Action OnReplaceModeStarted;
    public Action OnInventoryUpdated;
    public Action OnSpecialCardUpdated;

    // ──────────────────────────────── 재료 카드 ────────────────────────────────

    public bool AddCard(IngredientSO newCard)
    {
        if (isReplacing) return false;

        if (ownedCards.Count >= MAX_SLOTS)
        {
            pendingCard  = newCard;
            isReplacing  = true;
            OnReplaceModeStarted?.Invoke();
            return true;
        }

        ownedCards.Add(newCard);
        OnInventoryUpdated?.Invoke();
        return true;
    }

    public void ConfirmReplace(int discardIndex)
    {
        if (isReplacing && pendingCard != null && discardIndex >= 0 && discardIndex < ownedCards.Count)
        {
            ownedCards[discardIndex] = pendingCard;
            EndReplaceMode();
        }
    }

    public void CancelReplace()
    {
        if (isReplacing)
        {
            Debug.Log("새로 주운 카드를 버렸습니다.");
            EndReplaceMode();
        }
    }

    private void EndReplaceMode()
    {
        pendingCard = null;
        isReplacing = false;
        OnInventoryUpdated?.Invoke();
    }

    public void RemoveCard(int index)
    {
        if (index >= 0 && index < ownedCards.Count)
        {
            Debug.Log($"{gameObject.name}: {ownedCards[index].itemName} 카드를 버렸습니다.");
            ownedCards.RemoveAt(index);
        }
    }

    public List<IngredientSO> GetOwnedCards() => new List<IngredientSO>(ownedCards);

    // ──────────────────────────────── 특수 카드 ────────────────────────────────

    public void AddSpecialCard(SpecialCardSO card)
    {
        heldSpecialCard = card;
        OnSpecialCardUpdated?.Invoke();
        Debug.Log($"{gameObject.name}: 특수카드 [{card.cardName}] 보관");
    }

    public void ConsumeSpecialCard()
    {
        heldSpecialCard = null;
        OnSpecialCardUpdated?.Invoke();
    }

    // ──────────────────────────────── 특수 카드 효과 ─────────────────────────

    public void DropRandomIngredient()
    {
        if (ownedCards.Count == 0) return;
        int idx = UnityEngine.Random.Range(0, ownedCards.Count);
        Debug.Log($"<color=orange>{gameObject.name}: {ownedCards[idx].itemName} 재료를 흘렸습니다!</color>");
        ownedCards.RemoveAt(idx);
        OnInventoryUpdated?.Invoke();
    }

    public void MoldRandomIngredient()
    {
        if (ownedCards.Count == 0) return;

        if (heldSpecialCard != null && heldSpecialCard.cardType == SpecialCardType.FreshShield)
        {
            Debug.Log($"<color=cyan>{gameObject.name}: 신선 보호막으로 곰팡이 차단!</color>");
            ConsumeSpecialCard();
            return;
        }

        if (rottenIngredient == null)
        {
            Debug.LogWarning($"{gameObject.name}: rottenIngredient 미설정 — 인스펙터에서 썩은 재료 SO를 연결해주세요.");
            return;
        }

        int idx = UnityEngine.Random.Range(0, ownedCards.Count);
        Debug.Log($"<color=red>{gameObject.name}: {ownedCards[idx].itemName}에 곰팡이 발생! → 썩은 재료</color>");
        ownedCards[idx] = rottenIngredient;
        OnInventoryUpdated?.Invoke();
    }
}
