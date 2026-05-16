using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public KeyCode interactKey;
    public float detectRadius = 0.8f; // 감지 범위
    public LayerMask interactableLayer; // 'Basket' 레이어만 감지하도록 설정

    private Inventory inventory;
    private Vector2 lastMoveDir; // 캐릭터가 마지막으로 바라본 방향

    void Start() => inventory = GetComponent<Inventory>();

    void Update()
    {
        if (GamePhaseManager.Instance.currentPhase != GamePhase.Farming) return;

        // 이동 입력에서 방향 추출 (한결님 코드에서 moveInput 가져오기)
        float h = Input.GetAxisRaw(gameObject.name == "player1(cook)" ? "Horizontal" : "P2Horizontal");
        float v = Input.GetAxisRaw(gameObject.name == "player1(cook)" ? "Vertical" : "P2Vertical");

        if (h != 0 || v != 0) lastMoveDir = new Vector2(h, v).normalized;

        if (Input.GetKeyDown(interactKey))
        {
            CheckInteraction();
        }
    }

    void CheckInteraction()
    {
        // 플레이어 위치에서 바라보는 방향으로 원형 캐스트 (OverlapCircle)
        Collider2D hit = Physics2D.OverlapCircle((Vector2)transform.position + lastMoveDir * 0.5f, detectRadius, interactableLayer);

        if (hit != null)
        {
            IInteractable target = hit.GetComponent<IInteractable>();
            target?.OnInteract(inventory);
        }
    }

    // 에디터에서 범위를 시각적으로 확인용
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + lastMoveDir * 0.5f, detectRadius);
    }
}