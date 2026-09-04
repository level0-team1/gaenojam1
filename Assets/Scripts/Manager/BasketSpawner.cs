using System.Collections.Generic;
using UnityEngine;

public class BasketSpawner : MonoBehaviour
{
    public GameObject basketPrefab;
    public List<IngredientSO> ingredientPool;
    public List<SpecialCardSO> specialCardPool;
    [Range(0f, 1f)] public float specialCardChance = 0.2f;
    public int basketCount = 30;
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public float checkRadius = 0.3f;

    void Start()
    {
        for (int i = 0; i < basketCount; i++)
            SpawnBasket();
    }

    void SpawnBasket()
    {
        for (int i = 0; i < 20; i++)
        {
            float x = Random.Range(minBounds.x, maxBounds.x);
            float y = Random.Range(minBounds.y, maxBounds.y);
            Vector2 pos = new Vector2(x, y);

            if (!Physics2D.OverlapCircle(pos, checkRadius))
            {
                GameObject basketObj  = Instantiate(basketPrefab, pos, Quaternion.identity);
                Basket     basketScript = basketObj.GetComponent<Basket>();
                if (basketScript == null) return;

                bool spawnSpecial = specialCardPool != null && specialCardPool.Count > 0
                                    && Random.value < specialCardChance;
                if (spawnSpecial)
                {
                    basketScript.containedSpecialCard = specialCardPool[Random.Range(0, specialCardPool.Count)];
                    basketScript.containedIngredient  = null;
                }
                else if (ingredientPool.Count > 0)
                {
                    basketScript.containedIngredient  = ingredientPool[Random.Range(0, ingredientPool.Count)];
                    basketScript.containedSpecialCard = null;
                }
                return;
            }
        }
    }
}
