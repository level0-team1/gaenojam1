using UnityEngine;

public class Basket : MonoBehaviour
{
    public IngredientSO containedIngredient; // 스폰 시 할당될 재료
    private int touchCount = 0;
    private bool isPickedUp = false;

    // 플레이어가 상호작용할 때 호출되는 함수
    public void Interact(Inventory playerInventory)
    {
        if (isPickedUp) return;

        touchCount++;

        if (touchCount == 1)
        {
            // 첫 번째 터치: 내용물 확인 (민희님 아이콘 UI나 디버그 로그)
            Debug.Log($"바구니 확인: {containedIngredient.itemName}이(가) 들어있습니다!");
            // TODO: 바구니 위에 아이콘을 띄우는 연출 추가
        }
        else if (touchCount == 2)
        {
            // 두 번째 터치: 인벤토리 추가 시도
            if (playerInventory.AddCard(containedIngredient))
            {
                isPickedUp = true;
                Debug.Log($"{containedIngredient.itemName} 획득 완료!");
                Destroy(gameObject); // 혹은 비활성화
            }
        }
    }
}