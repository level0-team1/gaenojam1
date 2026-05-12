using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // 최대 소지 가능한 식재료 카드 수 
    private const int MAX_SLOTS = 6;

    // 현재 플레이어가 보유한 식재료 카드 리스트
    // (Dictionary보다 List가 6칸 UI 인덱스 매칭에 더 유리합니다)
    [SerializeField] private List<IngredientSO> ownedCards = new List<IngredientSO>();

    // 식재료 카드 추가 (바구니 상호작용 시 호출) [cite: 44, 45, 73]
    public bool AddCard(IngredientSO newCard)
    {
        // 1. 인벤토리 풀 체크 
        if (ownedCards.Count >= MAX_SLOTS)
        {
            Debug.Log($"{gameObject.name}: 인벤토리가 가득 찼습니다! (6/6)");
            return false; // 추가 실패
        }

        // 2. 카드 추가
        ownedCards.Add(newCard);
        Debug.Log($"{gameObject.name}: {newCard.itemName} 카드 획득! (현재: {ownedCards.Count}/{MAX_SLOTS})");

        // UI 갱신 로직이 있다면 여기서 호출 (팀원 태스크) [cite: 53, 70]
        return true;
    }

    // 카드 버리기/교체 로직 (필요 시 호출)
    public void RemoveCard(int index)
    {
        if (index >= 0 && index < ownedCards.Count)
        {
            Debug.Log($"{gameObject.name}: {ownedCards[index].itemName} 카드를 버렸습니다.");
            ownedCards.RemoveAt(index);
        }
    }

    // 조리 화면으로 현재 카드 목록을 넘겨주기 위한 함수 [cite: 22, 39]
    public List<IngredientSO> GetOwnedCards()
    {
        return new List<IngredientSO>(ownedCards);
    }

    // 특정 재료를 몇 개 가지고 있는지 확인 (UI 힌트나 체크용) [cite: 1, 49, 79]
    public int GetIngredientCount(IngredientSO target)
    {
        int count = 0;
        foreach (var card in ownedCards)
        {
            if (card == target) count++;
        }
        return count;
    }

    // 기존의 CheckVictory는 삭제되었습니다. 
    // 승리 판정은 1분 후 '조리 화면'에서 ScoringSystem이 수행합니다. [cite: 13, 36]
}