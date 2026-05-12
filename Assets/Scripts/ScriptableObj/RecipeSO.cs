using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IngredientAmount
{
    public IngredientSO ingredient;
    public int amount;
}

[CreateAssetMenu(fileName = "NewRecipe", menuName = "CookingGame/Recipe")]
public class RecipeSO : ScriptableObject
{
    public string recipeName;
    public Sprite resultImage;
    public List<IngredientAmount> requiredIngredients; // 정답 재료 리스트

    [Header("추리 시스템 설정")]
    public int revealCount = 2; // 처음부터 보여줄 재료 개수 (예: 2개)

    // 이 레시피와 유사한 '실패 요리'들의 점수 테이블 (선택 사항)
    // 혹은 로직으로 계산할 수도 있습니다.
}