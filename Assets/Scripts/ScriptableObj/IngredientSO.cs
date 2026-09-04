    using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "CookingGame/Ingredient")]
public class IngredientSO : ScriptableObject
{
    public string itemName;
    public enum Category { Meat, Veggie, Seasoning, Base }
    public Category category;
    public Sprite icon;
    [Tooltip("곰팡이 카드로 교체된 썩은 재료 여부 — ScoringSystem에서 -15점 감점")]
    public bool isRotten = false;
}