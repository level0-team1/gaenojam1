using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    public int CalculateCookingScore(List<IngredientSO> potCards, RecipeSO targetRecipe)
    {
        float totalScore = 0;
        int correctTypeCount = 0;
        int perfectAmountCount = 0;

        // 1. 플레이어가 냄비에 넣은 재료들의 수량 파악
        Dictionary<IngredientSO, int> playerIngredients = new Dictionary<IngredientSO, int>();
        foreach (var card in potCards)
        {
            if (playerIngredients.ContainsKey(card)) playerIngredients[card]++;
            else playerIngredients.Add(card, 1);
        }

        // 2. 레시피와 비교
        foreach (var required in targetRecipe.requiredIngredients)
        {
            if (playerIngredients.ContainsKey(required.ingredient))
            {
                // 재료 종류를 맞춘 경우
                correctTypeCount++;

                // 수량까지 완벽히 맞춘 경우 (보너스)
                if (playerIngredients[required.ingredient] == required.amount)
                {
                    perfectAmountCount++;
                }
            }
        }

        // 3. 점수 합산 (가중치 조절 가능)
        float typeScore = (float)correctTypeCount / targetRecipe.requiredIngredients.Count * 80f;
        float amountScore = (float)perfectAmountCount / targetRecipe.requiredIngredients.Count * 20f;

        totalScore = typeScore + amountScore;

        // 4. 오답 재료 감점 로직 (선택 사항)
        foreach (var playerItem in playerIngredients.Keys)
        {
            bool isRequired = targetRecipe.requiredIngredients.Exists(x => x.ingredient == playerItem);
            if (!isRequired) totalScore -= 5f; // 레시피에 없는 재료 하나당 5점 감점
        }

        return Mathf.Clamp(Mathf.RoundToInt(totalScore), 0, 100);
    }
}