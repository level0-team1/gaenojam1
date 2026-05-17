using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey;
    public float detectRadius = 0.8f; // 감지 범위
    public LayerMask interactableLayer; // 'Basket' 레이어만 감지하도록 설정

    private Inventory inventory;
    private Vector2 lastMoveDir; // 캐릭터가 마지막으로 바라본 방향

    // 💡 추가된 타겟팅 관련 변수
    private Basket currentTargetBasket;
    private bool isInteracting = false;

    void Start() => inventory = GetComponent<Inventory>();

    void Update()
    {
        // 💡 상호작용 불가 상태면 타겟팅 지우고 중단
        if (GamePhaseManager.Instance.currentPhase != GamePhase.Farming || inventory.isUIOpen || isInteracting)
        {
            ClearCurrentTarget();
            return;
        }

        // 이동 입력에서 방향 추출
        float h = Input.GetAxisRaw(gameObject.name == "player1(cook)" ? "Horizontal" : "P2Horizontal");
        float v = Input.GetAxisRaw(gameObject.name == "player1(cook)" ? "Vertical" : "P2Vertical");

        if (h != 0 || v != 0) lastMoveDir = new Vector2(h, v).normalized;

        // 💡 매 프레임 시야(바라보는 방향) 내에서 가장 가까운 바구니 탐색
        FindClosestBasket();

        if (Input.GetKeyDown(interactKey))
        {
            InteractWithBasket();
        }
    }

    // 💡 가장 가까운 바구니를 찾고 화살표 아이콘을 띄워주는 핵심 함수
    private void FindClosestBasket()
    {
        Vector2 checkPos = (Vector2)transform.position + lastMoveDir * 0.5f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, detectRadius, interactableLayer);

        Basket closestBasket = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider2D hit in hits)
        {
            Basket basket = hit.GetComponent<Basket>();
            if (basket == null) continue;

            // 캐릭터 중심에서 바구니까지의 실제 거리 계산
            float distance = Vector2.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestBasket = basket;
            }
        }

        // 💡 탐색된 바구니가 이전과 달라졌을 때만 갱신 (퍼포먼스 최적화)
        if (closestBasket != currentTargetBasket)
        {
            ClearCurrentTarget(); // 기존 아이콘 끄기

            currentTargetBasket = closestBasket;
            if (currentTargetBasket != null)
            {
                currentTargetBasket.SetHighlight(true); // 새 아이콘 켜기
            }
        }
    }

    private void InteractWithBasket()
    {
        // 타겟팅된 바구니가 있다면 상호작용 실행
        if (currentTargetBasket != null)
        {
            isInteracting = true;

            // 💡 1. 지우기 전에 바구니의 상호작용 컴포넌트를 미리 빼서 안전한 곳(target)에 보관합니다.
            IInteractable target = currentTargetBasket.GetComponent<IInteractable>();

            // 💡 2. 이제 안심하고 타겟팅 정보를 지우고 화살표를 끕니다.
            ClearCurrentTarget();

            // 💡 3. 아까 보관해둔 컴포넌트(target)를 써서 바구니를 뒤집습니다!
            target?.OnInteract(inventory);

            // 뒤집기 애니메이션 시간(약 0.8초) 동안 타겟팅 방지 후 다시 탐색 허용
            Invoke(nameof(EndInteraction), 0.8f);
        }
    }

    private void EndInteraction()
    {
        isInteracting = false;
    }

    private void ClearCurrentTarget()
    {
        if (currentTargetBasket != null)
        {
            currentTargetBasket.SetHighlight(false);
            currentTargetBasket = null;
        }
    }

    // 에디터에서 범위를 시각적으로 확인용
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + lastMoveDir * 0.5f, detectRadius);
    }
}