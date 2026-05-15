using System.Collections.Generic; // 추가
using UnityEditor.VersionControl;
using UnityEngine;

public class BasketSpawner : MonoBehaviour
{
    public GameObject basketPrefab;
    public List<IngredientSO> ingredientPool; // 유민님이 만든 10종 재료 리스트
    public int basketCount = 30;
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public float checkRadius = 0.3f;

    void Start()
    {
        for (int i = 0; i < basketCount; i++)
        {
            SpawnBasket();
        }
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
                GameObject basketObj = Instantiate(basketPrefab, pos, Quaternion.identity);

                // 생성된 바구니에 랜덤 재료 주입
                Basket basketScript = basketObj.GetComponent<Basket>();
                if (basketScript != null && ingredientPool.Count > 0)
                {
                    int randomIndex = Random.Range(0, ingredientPool.Count);
                    basketScript.containedIngredient = ingredientPool[randomIndex];
                }
                return;
            }
        }
    }
}