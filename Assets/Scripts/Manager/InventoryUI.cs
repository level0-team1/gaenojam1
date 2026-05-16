using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public Inventory targetInventory;
    public KeyCode toggleKey = KeyCode.Tab;
    public KeyCode confirmKey = KeyCode.F; // 인스펙터에서 F 또는 L로 설정
    public GameObject uiPanel;

    [Header("카드 슬롯 설정")]
    public List<Image> cardIcons; // 식재료 이미지 리스트

    private bool isVisible = false;
    private int selectedIndex = 0;

    // 💡 추가된 변수: 창이 열린 시간을 기록
    private float replaceOpenTime = 0f;

    void Start()
    {
        if (uiPanel != null) uiPanel.SetActive(false);

        if (targetInventory != null)
        {
            targetInventory.OnReplaceModeStarted += StartReplaceMode;
            targetInventory.OnInventoryUpdated += UpdateUI;
        }
    }

    void Update()
    {
        if (targetInventory != null && targetInventory.isReplacing)
        {
            HandleReplaceInput();
            return;
        }

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }

    private void StartReplaceMode()
    {
        isVisible = true;
        uiPanel.SetActive(true);
        selectedIndex = 0;

        // 💡 창이 열린 현재 시간을 기록
        replaceOpenTime = Time.time;

        UpdateUI();
    }

    private void HandleReplaceInput()
    {
        // 💡 핵심 방어 로직: 창이 열리고 0.2초 안에는 입력(F키)을 무시함
        if (Time.time - replaceOpenTime < 0.2f) return;

        // 좌우 방향키로 버릴 카드 선택
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = 4;
            UpdateUI();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex++;
            if (selectedIndex > 4) selectedIndex = 0;
            UpdateUI();
        }

        // F 또는 L키를 눌러 교체 확정
        if (Input.GetKeyDown(confirmKey))
        {
            targetInventory.ConfirmReplace(selectedIndex);
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
            Image currentSlot = cardIcons[i];

            if (i < cards.Count)
            {
                currentSlot.gameObject.SetActive(true);
                currentSlot.sprite = cards[i].icon;

                if (targetInventory.isReplacing && i == selectedIndex)
                {
                    currentSlot.transform.localScale = Vector3.one * 1.15f;
                    currentSlot.color = new Color(1f, 0.6f, 0.6f);
                }
                else
                {
                    currentSlot.transform.localScale = Vector3.one;
                    currentSlot.color = Color.white;
                }
            }
            else
            {
                currentSlot.gameObject.SetActive(false);
                currentSlot.transform.localScale = Vector3.one;
                currentSlot.color = Color.white;
            }
        }
    }
}