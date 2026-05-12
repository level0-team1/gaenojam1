using UnityEngine;

public class BasketSpawner : MonoBehaviour
{
    public GameObject basketPrefab;

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
                Instantiate(basketPrefab, pos, Quaternion.identity);
                Debug.Log("생성 성공: " + pos);
                return;
            }
        }

        Debug.LogWarning("스폰 실패 (공간 없음)");
    }
}