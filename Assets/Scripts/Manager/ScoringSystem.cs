using System.Collections.Generic;
using UnityEngine;

public class ScoringSystem : MonoBehaviour
{
    public int CalculateCookingScore(List<IngredientSO> potCards, RecipeSO targetRecipe)
    {
        if (targetRecipe == null || targetRecipe.requiredIngredients.Count == 0) return 0;

        // 💡 핵심 기법: 플레이어가 넣은 재료들을 복사한 리스트를 만듭니다.
        // 채점이 완료된 재료는 여기서 하나씩 제거하여 중복 채점을 방지합니다.
        List<IngredientSO> playerPotItems = new List<IngredientSO>(potCards);

        float totalScore = 0f;
        int totalRequiredSlots = targetRecipe.requiredIngredients.Count;

        // 밸런싱 조절을 위한 가중치 변수 (기본 100점 만점 기준 분포)
        // 기존 기획: 종류 맞춤(80점 만점) + 수량 맞춤(20점 만점)
        float basePointsPerSlot = 80f / totalRequiredSlots;
        float bonusPointsPerSlot = 20f / totalRequiredSlots;

        Debug.Log($"<color=cyan>[채점 시작] 오늘의 요리: {targetRecipe.recipeName}</color>");

        // ------------------------------------------------------------
        // 1단계: [완벽 일치 (Exact Match)] 검사
        // 정답 재료가 올바르게 들어갔는지 먼저 확인하고 리스트에서 제거합니다.
        // ------------------------------------------------------------
        foreach (var required in targetRecipe.requiredIngredients)
        {
            IngredientSO reqIngredient = required.ingredient;
            int reqAmount = required.amount;

            // 플레이어가 이 정답 재료를 총 몇 개 넣었는지 카운트
            int exactCountInPot = 0;
            foreach (var item in playerPotItems)
            {
                if (item == reqIngredient) exactCountInPot++;
            }

            if (exactCountInPot > 0)
            {
                // 정답 재료 종류를 최소 1개 이상 넣었으므로 기본 점수 획득!
                totalScore += basePointsPerSlot;
                Debug.Log($"[정답 인정] {reqIngredient.itemName} 발견! (+{basePointsPerSlot:F1}점)");

                // 수량까지 완벽하게 일치하는지 체크 (보너스)
                if (exactCountInPot == reqAmount)
                {
                    totalScore += bonusPointsPerSlot;
                    Debug.Log($"[수량 보너스] {reqIngredient.itemName} 개수({reqAmount}개) 완벽 일치! (+{bonusPointsPerSlot:F1}점)");
                }

                // 매칭 완료된 정답 재료는 필요한 수량(또는 넣은 수량 중 작은 값)만큼 플레이어 리스트에서 제거
                int itemsToRemove = Mathf.Min(exactCountInPot, reqAmount);
                for (int i = 0; i < itemsToRemove; i++)
                {
                    playerPotItems.Remove(reqIngredient);
                }
            }
        }

        // ------------------------------------------------------------
        // 2단계: [유사도 판정 (Category Match)] 검사
        // 1단계에서 완벽 일치로 안 거 걸러진 '남은 요구 사항'들에 대해 태그(Category) 매칭을 수행합니다.
        // ------------------------------------------------------------
        foreach (var required in targetRecipe.requiredIngredients)
        {
            IngredientSO reqIngredient = required.ingredient;
            IngredientSO.Category reqCategory = reqIngredient.category;

            // 이미 1단계에서 정답 재료로 처리된 녀석은 패스하기 위해,
            // 현재 플레이어 냄비에 정답 재료가 '아예 안 남아있는' 경우에만 카테고리 매칭을 시도합니다.
            bool alreadyMatched = !playerPotItems.Contains(reqIngredient);

            // 만약 플레이어가 아예 안 넣어서 1단계에서 패스된 정답 재료라면 대체재를 찾습니다.
            // (예: 소고기가 필요한데 냄비에 소고기가 없는 상태)
            if (potCards.FindAll(x => x == reqIngredient).Count == 0)
            {
                IngredientSO substituteItem = null;

                // 남은 플레이어 재료 중 같은 카테고리(태그)를 가진 녀석이 있는지 탐색
                foreach (var item in playerPotItems)
                {
                    if (item.category == reqCategory)
                    {
                        substituteItem = item;
                        break;
                    }
                }

                // 💡 기획서 핵심 구현: 대체 재료를 찾았다면 부분 점수 부여!
                if (substituteItem != null)
                {
                    totalScore += 10f; // 유민님 기획: 같은 카테고리 부분 점수 +10점 
                    Debug.Log($"[유사도 인정] 정답({reqIngredient.itemName}) 대신 같은 {reqCategory}류인 [{substituteItem.itemName}] 감지! 부분 점수 (+10점) 부여");

                    // 대체에 사용된 재료도 냄비 리스트에서 제거하여 중복 소모 방지
                    playerPotItems.Remove(substituteItem);
                }
            }
        }

        // ------------------------------------------------------------
        // 3단계: [오답 감점 (Penalty)] 처리
        // 정답으로도, 카테고리 대체재로도 쓰이지 못하고 냄비에 남은 찌꺼기/과다 재료 감점
        // ------------------------------------------------------------
        foreach (var leftoverItem in playerPotItems)
        {
            totalScore -= 5f; // 레시피와 전혀 상관없는 재료 하나당 5점 감점
            Debug.Log($"<color=red>[오답 감점] 레시피와 무관한 재료 포함: {leftoverItem.itemName} (-5점)</color>");
        }

        // 최종 점수를 0점에서 100점 사이로 제한하여 반올림 후 리턴
        int finalScore = Mathf.Clamp(Mathf.RoundToInt(totalScore), 0, 100);
        Debug.Log($"<color=yellow>[채점 완료] 최종 Cooking 점수: {finalScore}점</color>");

        return finalScore;
    }
}