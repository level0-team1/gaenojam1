using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // 💡 최대 슬롯을 기획서에 맞게 5칸으로 수정
    private const int MAX_SLOTS = 5;

    [SerializeField] private List<IngredientSO> ownedCards = new List<IngredientSO>();

    [Header("교체 시스템")]
    public bool isReplacing = false; // 현재 교체 중인지 여부
    public IngredientSO pendingCard { get; private set; } // 바구니에서 막 꺼낸 새 재료

    // UI와 소통하기 위한 이벤트 (실무에서 자주 쓰는 방식)
    public Action OnReplaceModeStarted;
    public Action OnInventoryUpdated;

    public bool AddCard(IngredientSO newCard)
    {
        // 1. 이미 교체 중이면 다른 바구니를 먹지 못하게 막음
        if (isReplacing) return false;

        // 2. 인벤토리가 꽉 찼을 때 -> 교체 모드 돌입
        if (ownedCards.Count >= MAX_SLOTS)
        {
            Debug.Log($"{gameObject.name}: 인벤토리 가득 참! 교체 모드 진입.");
            pendingCard = newCard;
            isReplacing = true;

            OnReplaceModeStarted?.Invoke(); // UI에 "교체창 띄워!"라고 알림
            StartCoroutine(ReplaceTimerCoroutine()); // 💡 기획서 룰: 2초 타이머 시작

            return true; // 바구니는 맵에서 없어져야 하므로 true 반환
        }

        // 3. 자리가 있을 때 -> 평범하게 추가
        ownedCards.Add(newCard);
        Debug.Log($"{gameObject.name}: {newCard.itemName} 획득! (현재: {ownedCards.Count}/{MAX_SLOTS})");
        OnInventoryUpdated?.Invoke();
        return true;
    }

    // 플레이어가 버릴 카드를 선택하고 'E'를 눌렀을 때 호출됨
    public void ConfirmReplace(int discardIndex)
    {
        if (isReplacing && pendingCard != null && discardIndex >= 0 && discardIndex < ownedCards.Count)
        {
            Debug.Log($"{ownedCards[discardIndex].itemName}을(를) 버리고 {pendingCard.itemName} 획득!");
            ownedCards[discardIndex] = pendingCard; // 카드 교체
            EndReplaceMode();
        }
    }

    // 2초 초과 시 자동으로 새 재료를 버리는 로직
    private IEnumerator ReplaceTimerCoroutine()
    {
        yield return new WaitForSeconds(2.0f);

        if (isReplacing)
        {
            Debug.Log("2초 경과! 교체하지 않고 새 재료를 버립니다.");
            EndReplaceMode();
        }
    }

    private void EndReplaceMode()
    {
        pendingCard = null;
        isReplacing = false;
        OnInventoryUpdated?.Invoke(); // UI 원래대로 복구
    }

    public List<IngredientSO> GetOwnedCards()
    {
        return new List<IngredientSO>(ownedCards);
    }
}