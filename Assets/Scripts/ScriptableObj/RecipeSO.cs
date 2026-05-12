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
    public Sprite resultImage; // 완성된 요리 이미지 [cite: 24]
    public List<IngredientAmount> requiredIngredients; // 필요 재료 리스트
}