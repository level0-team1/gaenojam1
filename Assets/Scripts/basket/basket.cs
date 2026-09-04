using System.Collections;
using UnityEngine;

public class Basket : MonoBehaviour, IInteractable
{
    [Header("내용물 (둘 중 하나만 설정)")]
    public IngredientSO containedIngredient;
    public SpecialCardSO containedSpecialCard;

    private Animator anim;
    private enum BasketState { Closed, Peeked, Empty }
    private BasketState currentState = BasketState.Closed;
    private bool isAnimating = false;
    private Inventory lastInteractor;

    [Header("타겟 표시 UI")]
    public GameObject targetIndicator;

    [Header("특수카드 애니메이션")]
    public float specialOpenDuration = 0.5f;

    private bool IsSpecial => containedSpecialCard != null;

    void Awake() => anim = GetComponent<Animator>();

    public void OnInteract(Inventory playerInventory)
    {
        if (isAnimating || currentState == BasketState.Empty) return;
        lastInteractor = playerInventory;

        if (currentState == BasketState.Closed)
            StartCoroutine(PeekSequence());
        else if (currentState == BasketState.Peeked)
            StartCoroutine(CollectSequence(playerInventory));
    }

    public void SetHighlight(bool isActive)
    {
        if (targetIndicator != null) targetIndicator.SetActive(isActive);
    }

    IEnumerator PeekSequence()
    {
        isAnimating = true;

        if (IsSpecial)
        {
            anim.SetTrigger("OpenSpecial");
            Debug.Log($"[바구니] 특수카드 발견: {containedSpecialCard.cardName}");
            yield return new WaitForSeconds(specialOpenDuration);

            var selfStatus2 = lastInteractor.GetComponent<PlayerStatus>();
            if (containedSpecialCard.isInstant)
            {
                TriggerInstantCard(containedSpecialCard, lastInteractor);
            }
            else
            {
                lastInteractor.AddSpecialCard(containedSpecialCard);
                if (selfStatus2 != null)
                    SpecialCardNotify.ShowOnCanvas(selfStatus2.playerCanvas, containedSpecialCard);
            }

            currentState = BasketState.Empty;
            isAnimating = false;
            Destroy(gameObject);
            yield break;
        }

        // 일반 재료 바구니
        Debug.Log($"[바구니] 내용물 확인: {containedIngredient.itemName}");
        anim.SetTrigger("Open");
        var selfStatus = lastInteractor.GetComponent<PlayerStatus>();
        if (selfStatus != null)
            IngredientCardNotify.Show(selfStatus.playerCanvas, containedIngredient);
        yield return new WaitForSeconds(0.8f);
        anim.SetTrigger("Close");
        yield return new WaitForSeconds(0.5f);
        currentState = BasketState.Peeked;
        isAnimating = false;
    }

    IEnumerator CollectSequence(Inventory playerInventory)
    {
        isAnimating = true;
        anim.SetTrigger("Open");

        if (IsSpecial)
        {
            // 특수카드는 PeekSequence에서 즉시 처리되므로 여기 도달하지 않음
            yield return new WaitForSeconds(0.3f);
            playerInventory.AddSpecialCard(containedSpecialCard);
            currentState = BasketState.Empty;
            Destroy(gameObject);
        }
        else
        {
            if (playerInventory.AddCard(containedIngredient))
            {
                yield return new WaitForSeconds(0.3f);
                currentState = BasketState.Empty;
                Destroy(gameObject);
            }
            else
            {
                anim.SetTrigger("Close");
                yield return new WaitForSeconds(0.5f);
                isAnimating = false;
            }
        }
    }

    private void TriggerInstantCard(SpecialCardSO card, Inventory self)
    {
        PlayerStatus selfStatus = self.GetComponent<PlayerStatus>();
        Inventory    oppInv     = GetOpponentInventory(self);
        PlayerStatus oppStatus  = oppInv?.GetComponent<PlayerStatus>();

        bool isDebuff = false;
        bool isSelfBuff = false;

        switch (card.cardType)
        {
            case SpecialCardType.RocketBasket:
                selfStatus?.ApplySpeedBoost(2f, 5f);
                isSelfBuff = true;
                break;
            case SpecialCardType.SlowBasket:
                oppStatus?.ApplySpeedBoost(0.4f, 3f);
                isDebuff = true;
                break;
            case SpecialCardType.Blackout:
                oppStatus?.ApplyBlackout(3f);
                isDebuff = true;
                break;
            case SpecialCardType.ControlReverse:
                oppStatus?.ApplyControlReverse(3f);
                isDebuff = true;
                break;
            case SpecialCardType.Stun:
                oppStatus?.ApplyStun(1f);
                isDebuff = true;
                break;
            case SpecialCardType.DropIngredient:
                oppInv?.DropRandomIngredient();
                isDebuff = true;
                break;
            case SpecialCardType.Mold:
                oppInv?.MoldRandomIngredient();
                isDebuff = true;
                break;
        }

        if (isDebuff && oppStatus != null)
            SpecialCardNotify.ShowOnCanvas(oppStatus.playerCanvas, card);
        if (isSelfBuff && selfStatus != null)
            SpecialCardNotify.ShowOnCanvas(selfStatus.playerCanvas, card);

        Debug.Log($"[특수카드] <color=magenta>{card.cardName}</color> 발동! 사용자: {self.gameObject.name}");
    }

    private Inventory GetOpponentInventory(Inventory self)
    {
        var allInvs = FindObjectsByType<Inventory>(FindObjectsSortMode.None);
        foreach (var inv in allInvs)
            if (inv != self) return inv;
        return null;
    }
}
