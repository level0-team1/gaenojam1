using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    public Inventory targetInventory;
    public KeyCode toggleKey = KeyCode.Tab;
    public KeyCode confirmKey = KeyCode.F;
    public KeyCode prevKey = KeyCode.A;
    public KeyCode nextKey = KeyCode.D;
    [Tooltip("prevKey 대체 키 (P1=W, P2=UpArrow)")]
    public KeyCode altPrevKey = KeyCode.W;
    [Tooltip("nextKey 대체 키 (P1=S, P2=DownArrow)")]
    public KeyCode altNextKey = KeyCode.S;
    public GameObject uiPanel;

    [Header("스크롤")]
    public ScrollRect cardScrollRect;

    [Header("카드 슬롯 설정")]
    public List<Image> cardIcons;
    public List<TMP_Text> cardTexts;
    // 프리팹 자식에 새로 만든 'SelectionBorder' 이미지들을 여기에 연결합니다.
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

            // 💡기존 Input.GetKeyDown 대신 엔터 호환 검사 함수 사용
            if (IsToggleKeyPressed() && !targetInventory.isReplacing)
            {
                CloseInventory();
            }
            return;
        }

        // 💡기존 Input.GetKeyDown 대신 엔터 호환 검사 함수 사용
        if (IsToggleKeyPressed() && !targetInventory.isReplacing)
        {
            OpenInventory();
        }
    }

    // 💡 NEW: 텐키리스(TKL) 키보드 유저를 위해 두 종류의 엔터키를 상호 호환해주는 로직
    private bool IsToggleKeyPressed()
    {
        if (Input.GetKeyDown(toggleKey)) return true;

        // 인스펙터 설정이 키패드 엔터인데 일반 엔터를 누른 경우 혹은 그 반대 처리
        if (toggleKey == KeyCode.KeypadEnter && Input.GetKeyDown(KeyCode.Return)) return true;
        if (toggleKey == KeyCode.Return && Input.GetKeyDown(KeyCode.KeypadEnter)) return true;

        return false;
    }

    // 💡 NEW: 혹시 모를 확정 키 엔터 매핑을 위한 상호 호환 로직
    private bool IsConfirmKeyPressed()
    {
        if (Input.GetKeyDown(confirmKey)) return true;
        if (confirmKey == KeyCode.KeypadEnter && Input.GetKeyDown(KeyCode.Return)) return true;
        if (confirmKey == KeyCode.Return && Input.GetKeyDown(KeyCode.KeypadEnter)) return true;

        return false;
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

        if (Input.GetKeyDown(prevKey) || Input.GetKeyDown(altPrevKey))
        {
            selectedIndex--;
            if (selectedIndex < 0) selectedIndex = maxSelectionCount - 1;
            UpdateUI();
            ScrollToSelected();
        }
        else if (Input.GetKeyDown(nextKey) || Input.GetKeyDown(altNextKey))
        {
            selectedIndex++;
            if (selectedIndex >= maxSelectionCount) selectedIndex = 0;
            UpdateUI();
            ScrollToSelected();
        }

        // 💡 호환 검사 함수 적용
        if (IsConfirmKeyPressed())
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

                if (isVisible && i == selectedIndex)
                {
                    frameImg.transform.localScale = Vector3.one * 1.15f;
                    frameImg.color = Color.white;

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

                if (highlightImg != null) highlightImg.gameObject.SetActive(false);
            }
        }

        if (targetInventory.isReplacing && targetInventory.pendingCard != null)
        {
            if (pendingCardUI != null) pendingCardUI.SetActive(true);

            if (pendingIcon != null) pendingIcon.sprite = targetInventory.pendingCard.icon;
            if (pendingText != null) pendingText.text = targetInventory.pendingCard.itemName;

            Image pendingFrameImg = pendingCardUI.GetComponent<Image>();

            Transform pendingHighlightTransform = pendingCardUI.transform.Find("SelectionBorder");
            Image pendingHighlightImg = (pendingHighlightTransform != null) ? pendingHighlightTransform.GetComponent<Image>() : null;

            if (pendingFrameImg != null)
            {
                if (isVisible && selectedIndex == cards.Count)
                {
                    pendingFrameImg.transform.localScale = Vector3.one * 1.15f;
                    pendingFrameImg.color = Color.white;

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

                    if (pendingHighlightImg != null) pendingHighlightImg.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (pendingCardUI != null) pendingCardUI.SetActive(false);
        }
    }

    private void ScrollToSelected()
    {
        if (cardScrollRect == null || cardIcons.Count <= 1) return;
        // 0 ~ 1 정규화: 첫 카드=0, 마지막=1
        float total = targetInventory.isReplacing ? cardIcons.Count : cardIcons.Count - 1;
        if (total <= 0) return;
        float normalized = Mathf.Clamp01((float)selectedIndex / total);
        // 가로 스크롤이면 horizontal, 세로면 vertical 사용
        if (cardScrollRect.horizontal)
            StartCoroutine(SmoothScroll(true, normalized));
        else
            StartCoroutine(SmoothScroll(false, 1f - normalized)); // 세로는 위=1
    }

    private IEnumerator SmoothScroll(bool horizontal, float target)
    {
        float elapsed = 0f;
        float start = horizontal ? cardScrollRect.horizontalNormalizedPosition
                                 : cardScrollRect.verticalNormalizedPosition;
        while (elapsed < 0.15f)
        {
            elapsed += Time.deltaTime;
            float val = Mathf.Lerp(start, target, elapsed / 0.15f);
            if (horizontal) cardScrollRect.horizontalNormalizedPosition = val;
            else            cardScrollRect.verticalNormalizedPosition   = val;
            yield return null;
        }
        if (horizontal) cardScrollRect.horizontalNormalizedPosition = target;
        else            cardScrollRect.verticalNormalizedPosition   = target;
    }
}