using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory targetInventory;
    public KeyCode toggleKey;
    public GameObject uiPanel;        // 인벤토리 팝업 창

    [Header("카드 슬롯 설정")]
    public List<Image> cardIcons;     // 슬롯 내의 식재료 아이콘 Image들
    public Color emptyColor = new Color(1, 1, 1, 0); // 빈 칸은 투명하게

    private bool isVisible = false;

    void Start()
    {
        if (uiPanel != null) uiPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isVisible = !isVisible;
        uiPanel.SetActive(isVisible);

        if (isVisible) UpdateUI();
    }

    public void UpdateUI()
    {
        List<IngredientSO> cards = targetInventory.GetOwnedCards();

        for (int i = 0; i < cardIcons.Count; i++)
        {
            if (i < cards.Count)
            {
                // 1. 변수 이름을 Icon(대문자 주의)으로 수정
                cardIcons[i].sprite = cards[i].icon;
                cardIcons[i].color = Color.white;

                // 2. 카드가 있으면 슬롯 오브젝트를 활성화
                cardIcons[i].gameObject.SetActive(true);
            }
            else
            {
                // 3. 카드가 없으면 슬롯 오브젝트 자체를 숨김
                cardIcons[i].gameObject.SetActive(false);
            }
        }
    }
}