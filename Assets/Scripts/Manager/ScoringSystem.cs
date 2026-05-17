using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    public int CalculateCookingScore(List<IngredientSO> potCards, RecipeSO targetRecipe,
                                     Inventory inventory = null)
    {
        if (targetRecipe == null || targetRecipe.requiredIngredients.Count == 0) return 0;

        List<IngredientSO> playerPotItems = new List<IngredientSO>(potCards);

        float totalScore = 0f;
        int totalRequiredSlots = targetRecipe.requiredIngredients.Count;

        float basePointsPerSlot  = 80f / totalRequiredSlots;
        float bonusPointsPerSlot = 20f / totalRequiredSlots;

        Debug.Log($"<color=cyan>[채점 시작] 오늘의 요리: {targetRecipe.recipeName}</color>");

        // ── 썩은 재료 감점 처리 ──────────────────────────────────────────────
        foreach (var item in playerPotItems)
        {
            if (item != null && item.isRotten)
            {
                totalScore -= 15f;
                Debug.Log($"<color=red>[썩은 재료] {item.itemName} 포함 (-15점)</color>");
            }
        }
        playerPotItems.RemoveAll(x => x != null && x.isRotten);

        // ── 1단계: 완벽 일치 ─────────────────────────────────────────────────
        foreach (var required in targetRecipe.requiredIngredients)
        {
            IngredientSO reqIngredient = required.ingredient;
            int reqAmount = required.amount;

            int exactCountInPot = 0;
            foreach (var item in playerPotItems)
                if (item == reqIngredient) exactCountInPot++;

            if (exactCountInPot > 0)
            {
                totalScore += basePointsPerSlot;
                Debug.Log($"[정답 인정] {reqIngredient.itemName} 발견! (+{basePointsPerSlot:F1}점)");

                if (exactCountInPot == reqAmount)
                {
                    totalScore += bonusPointsPerSlot;
                    Debug.Log($"[수량 보너스] {reqIngredient.itemName} 개수 완벽 일치! (+{bonusPointsPerSlot:F1}점)");
                }

                int itemsToRemove = Mathf.Min(exactCountInPot, reqAmount);
                for (int i = 0; i < itemsToRemove; i++)
                    playerPotItems.Remove(reqIngredient);
            }
        }

        // ── 2단계: 카테고리 유사도 ──────────────────────────────────────────
        foreach (var required in targetRecipe.requiredIngredients)
        {
            IngredientSO reqIngredient = required.ingredient;
            IngredientSO.Category reqCategory = reqIngredient.category;

            if (potCards.FindAll(x => x == reqIngredient).Count == 0)
            {
                IngredientSO substituteItem = null;
                foreach (var item in playerPotItems)
                {
                    if (item.category == reqCategory) { substituteItem = item; break; }
                }

                if (substituteItem != null)
                {
                    totalScore += 10f;
                    Debug.Log($"[유사도 인정] {reqIngredient.itemName} 대신 {substituteItem.itemName} (+10점)");
                    playerPotItems.Remove(substituteItem);
                }
            }
        }

        // ── 3단계: 오답 감점 ─────────────────────────────────────────────────
        foreach (var leftoverItem in playerPotItems)
        {
            totalScore -= 5f;
            Debug.Log($"<color=red>[오답 감점] 무관 재료: {leftoverItem.itemName} (-5점)</color>");
        }

        // ── 보관 특수카드 효과 ───────────────────────────────────────────────
        if (inventory != null && inventory.heldSpecialCard != null)
        {
            switch (inventory.heldSpecialCard.cardType)
            {
                case SpecialCardType.MSG:
                    totalScore += 5f;
                    Debug.Log("[MSG] 비장의 조미료! (+5점)");
                    inventory.ConsumeSpecialCard();
                    break;

                case SpecialCardType.WildIngredient:
                    // 미충족 재료 슬롯이 하나라도 있으면 한 칸 채워줌
                    bool wildUsed = false;
                    foreach (var required in targetRecipe.requiredIngredients)
                    {
                        bool matched = false;
                        foreach (var card in potCards)
                            if (card == required.ingredient) { matched = true; break; }
                        if (!matched)
                        {
                            totalScore += basePointsPerSlot;
                            Debug.Log($"[만능 재료] {required.ingredient.itemName} 슬롯 와일드카드! (+{basePointsPerSlot:F1}점)");
                            wildUsed = true;
                            break;
                        }
                    }
                    if (wildUsed) inventory.ConsumeSpecialCard();
                    break;
            }
        }

        int finalScore = Mathf.Clamp(Mathf.RoundToInt(totalScore), 0, 100);
        Debug.Log($"<color=yellow>[채점 완료] 최종 점수: {finalScore}점</color>");
        return finalScore;
    }
}
