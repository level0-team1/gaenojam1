using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    public Inventory targetInventory;
    public KeyCode toggleKey = KeyCode.Tab;
    public KeyCode confirmKey = KeyCode.F;
    public GameObject uiPanel;

    [Header("카드 슬롯 설정")]
    public List<Image> cardIcons;
    public List<TMP_Text> cardTexts;
    // 💡 NEW: 프리팹 자식에 새로 만든 'SelectionBorder' 이미지들을 여기에 연결합니다.
    public List<Image> cardHighlights;

    [Header("교체용 추가 UI (주운 재료)")]
    public GameObject pendingCardUI;
    public Image pendingIcon;
    public TMP_Text pendingText;

    private bool isVisible = false;
    private int selectedIndex = 0;
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
        if (isVisible)
        {
            HandleUIInput();

            if (Input.GetKeyDown(toggleKey) && !targetInventory.isReplacing)
            {
                CloseInventory();
            }
            return;
        }

        if (Input.GetKeyDown(toggleKey) && !targetInventory.isReplacing)
        {
            OpenInventory();
        }
    }

    private void StartReplaceMode()
    {
        replaceOpenTime = Time.time;
        OpenInventory();
    }

    public void OpenInventory()
    {
        isVisible = true;
        uiPanel.SetActive(true);
        selectedIndex = 0;
        if (targetInventory != null) targetInventory.isUIOpen = true;
        UpdateUI();
    }

    public void CloseInventory()
    {
        isVisible = false;
        uiPanel.SetActive(false);
        if (targetInventory != null) targetInventory.isUIOpen = false;
    }

    private void HandleUIInput()
    {
        if (targetInventory.isReplacing && Time.time - replaceOpenTime < 0.2f) return;

        int cardCount = targetInventory.GetOwnedCards().Count;
        if (cardCount == 0) return;

        int maxSelectionCount = targetInventory.isReplacing ? cardCount + 1 : cardCount;

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = maxSelectionCount - 1;
            UpdateUI();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            selectedIndex++;
            if (selectedIndex >= maxSelectionCount) selectedIndex = 0;
            UpdateUI();
        }

        if (Input.GetKeyDown(confirmKey))
        {
            if (targetInventory.isReplacing)
            {
                if (selectedIndex == cardCount)
                {
                    targetInventory.CancelReplace();
                }
                else
                {
                    targetInventory.ConfirmReplace(selectedIndex);
                }
                CloseInventory();
            }
            else
            {
                targetInventory.RemoveCard(selectedIndex);

                if (selectedIndex >= targetInventory.GetOwnedCards().Count)
                    selectedIndex = Mathf.Max(0, targetInventory.GetOwnedCards().Count - 1);

                UpdateUI();
            }
        }
    }

    public void UpdateUI()
    {
        List<IngredientSO> cards = targetInventory.GetOwnedCards();

        for (int i = 0; i < cardIcons.Count; i++)
        {
            Image iconImg = cardIcons[i];
            if (iconImg == null) continue;

            Image frameImg = (iconImg.transform.parent != null) ? iconImg.transform.parent.GetComponent<Image>() : null;
            if (frameImg == null) frameImg = iconImg;

            // 💡 NEW: 이번 루프에 해당하는 하이라이트용 테두리 이미지를 가져옵니다.
            Image highlightImg = (i < cardHighlights.Count) ? cardHighlights[i] : null;

            if (i < cards.Count)
            {
                frameImg.gameObject.SetActive(true);
                iconImg.gameObject.SetActive(true);
                iconImg.sprite = cards[i].icon;

                if (i < cardTexts.Count && cardTexts[i] != null)
                {
                    cardTexts[i].text = cards[i].itemName;
                    cardTexts[i].gameObject.SetActive(true);
                }

                // 기존 카드 5장 중 하나가 선택되었을 때 효과
                if (isVisible && i == selectedIndex)
                {
                    frameImg.transform.localScale = Vector3.one * 1.15f;

                    // 💡 NEW 로직: 배경색(frameImg.color)은 건드리지 않습니다! 흰색 유지.
                    frameImg.color = Color.white;

                    // 💡 NEW 로직: 대신 별도로 분리한 테두리 이미지(highlightImg)를 켜고 색을 줍니다.
                    if (highlightImg != null)
                    {
                        highlightImg.gameObject.SetActive(true);
                        highlightImg.color = targetInventory.isReplacing ? new Color(1f, 0.6f, 0.6f) : new Color(0.6f, 0.8f, 1f);
                    }
                }
                else
                {
                    frameImg.transform.localScale = Vector3.one;
                    frameImg.color = Color.white;

                    // 💡 NEW 로직: 선택 안 됐을 때는 하이라이트 테두리를 끕니다.
                    if (highlightImg != null) highlightImg.gameObject.SetActive(false);
                }
            }
            else
            {
                frameImg.gameObject.SetActive(false);
                iconImg.gameObject.SetActive(false);

                if (i < cardTexts.Count && cardTexts[i] != null)
                {
                    cardTexts[i].gameObject.SetActive(false);
                }

                frameImg.transform.localScale = Vector3.one;
                frameImg.color = Color.white;

                // 💡 NEW 로직: 빈 칸일 때도 하이라이트 끔
                if (highlightImg != null) highlightImg.gameObject.SetActive(false);
            }
        }

        if (targetInventory.isReplacing && targetInventory.pendingCard != null)
        {
            if (pendingCardUI != null) pendingCardUI.SetActive(true);

            if (pendingIcon != null) pendingIcon.sprite = targetInventory.pendingCard.icon;
            if (pendingText != null) pendingText.text = targetInventory.pendingCard.itemName;

            // 새로 주운 카드(6번째 카드)가 선택되었을 때 시각 효과 주기
            Image pendingFrameImg = pendingCardUI.GetComponent<Image>();

            // 💡 NEW: PendingCard 프리팹 안에도 'SelectionBorder' 자식이 있어야 합니다!
            // PendingUI 자식 중에서 "SelectionBorder"라는 이름을 가진 Image 컴포넌트를 찾아옵니다.
            Transform pendingHighlightTransform = pendingCardUI.transform.Find("SelectionBorder");
            Image pendingHighlightImg = (pendingHighlightTransform != null) ? pendingHighlightTransform.GetComponent<Image>() : null;

            if (pendingFrameImg != null)
            {
                if (isVisible && selectedIndex == cards.Count) // 선택 인덱스가 5(마지막)일 때
                {
                    pendingFrameImg.transform.localScale = Vector3.one * 1.15f;
                    pendingFrameImg.color = Color.white; // 배경은 흰색 유지

                    // 💡 NEW: 주운 카드 전용 하이라이트 테두리를 켜고 색을 줍니다 (무조건 빨강)
                    if (pendingHighlightImg != null)
                    {
                        pendingHighlightImg.gameObject.SetActive(true);
                        pendingHighlightImg.color = new Color(1f, 0.6f, 0.6f);
                    }
                }
                else
                {
                    pendingFrameImg.transform.localScale = Vector3.one;
                    pendingFrameImg.color = Color.white;

                    // 💡 NEW: 선택 안 됐을 때는 주운 카드 하이라이트 끔
                    if (pendingHighlightImg != null) pendingHighlightImg.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (pendingCardUI != null) pendingCardUI.SetActive(false);
        }
    }
}