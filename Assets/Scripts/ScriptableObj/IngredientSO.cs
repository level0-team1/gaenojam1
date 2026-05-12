using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "CookingGame/Ingredient")]
public class IngredientSO : ScriptableObject
{
    public string itemName;
    public enum Category { Meat, Veggie, Seasoning, Base }
    public Category category;
    public Sprite icon;
}