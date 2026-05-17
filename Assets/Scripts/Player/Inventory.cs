using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    private const int MAX_SLOTS = 5;

    [SerializeField] private List<IngredientSO> ownedCards = new List<IngredientSO>();

    [Header("상태 플래그")]
    public bool isReplacing = false;
    public bool isUIOpen = false; // 💡 추가: 현재 인벤토리 창이 열려있는가? (이동 방지용)
    public IngredientSO pendingCard { get; private set; }

    public Action OnReplaceModeStarted;
    public Action OnInventoryUpdated;

    public bool AddCard(IngredientSO newCard)
    {
        if (isReplacing) return false;

        if (ownedCards.Count >= MAX_SLOTS)
        {
            pendingCard = newCard;
            isReplacing = true;

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

    public List<IngredientSO> GetOwnedCards()
    {
        return new List<IngredientSO>(ownedCards);
    }
}