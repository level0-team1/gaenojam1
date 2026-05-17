using System.Collections;
using UnityEngine;

public class Basket : MonoBehaviour, IInteractable
{
    public IngredientSO containedIngredient;
    private Animator anim;

    private enum BasketState { Closed, Peeked, Empty }
    private BasketState currentState = BasketState.Closed;
    private bool isAnimating = false;

    [Header("타겟 표시 UI")]
    public GameObject targetIndicator;

    void Awake() => anim = GetComponent<Animator>();

    public void OnInteract(Inventory playerInventory)
    {
        if (isAnimating || currentState == BasketState.Empty) return;

        if (currentState == BasketState.Closed)
            StartCoroutine(PeekSequence());
        else if (currentState == BasketState.Peeked)
            StartCoroutine(CollectSequence(playerInventory));
    }
    public void SetHighlight(bool isActive)
    {
        if (targetIndicator != null)
        {
            targetIndicator.SetActive(isActive);
        }
    }

    IEnumerator PeekSequence()
    {
        isAnimating = true;
        anim.SetTrigger("Open"); // 열기 애니메이션
        Debug.Log($"[실무 로그] 바구니 내용물 확인: {containedIngredient.itemName}");

        yield return new WaitForSeconds(0.8f); // 애니메이션 대기 (실무에선 Animation Event 추천)

        anim.SetTrigger("Close"); // 다시 닫기
        yield return new WaitForSeconds(0.5f);

        currentState = BasketState.Peeked;
        isAnimating = false;
    }

    IEnumerator CollectSequence(Inventory playerInventory)
    {
        isAnimating = true;
        anim.SetTrigger("Open");

        if (playerInventory.AddCard(containedIngredient))
        {
            yield return new WaitForSeconds(0.3f);
            currentState = BasketState.Empty;
            Destroy(gameObject); // 혹은 비활성화 및 이펙트 재생
        }
        else
        {
            // 인벤토리 꽉 찼을 때 처리
            anim.SetTrigger("Close");
            yield return new WaitForSeconds(0.5f);
            isAnimating = false;
        }
    }
}