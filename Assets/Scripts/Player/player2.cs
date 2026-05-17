using UnityEngine;

public class Player2Move : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator anim; // 💡 애니메이터 변수 추가
    private Vector2 moveInput;
    private Inventory inv;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // 💡 컴포넌트 가져오기
        inv = GetComponent<Inventory>();
    }

    void Update()
    {
        // 파밍 페이즈가 아닐 때는 입력을 막고 애니메이션도 강제로 Idle로 만듦
        if (GamePhaseManager.Instance.currentPhase != GamePhase.Farming || inv.isUIOpen)
        {
            moveInput = Vector2.zero;
            anim.SetFloat("Speed", 0);
            return;
        }

        moveInput.x = 0;
        moveInput.y = 0;

        if (Input.GetKey(KeyCode.UpArrow)) moveInput.y = 1;
        if (Input.GetKey(KeyCode.DownArrow)) moveInput.y = -1;
        if (Input.GetKey(KeyCode.LeftArrow)) moveInput.x = -1;
        if (Input.GetKey(KeyCode.RightArrow)) moveInput.x = 1;

        // 💡 애니메이션 데이터 전달의 핵심
        if (moveInput != Vector2.zero)
        {
            // 움직일 때만 방향 값을 넘겨줍니다.
            // 이렇게 해야 키보드에서 손을 떼어 멈췄을 때, 마지막으로 보던 방향의 Idle 애니메이션이 나옵니다.
            anim.SetFloat("Dirx", moveInput.x);
            anim.SetFloat("Diry", moveInput.y);
        }

        // 현재 속도를 넘겨주어 Idle과 Walk를 구분하게 합니다.
        anim.SetFloat("Speed", moveInput.magnitude);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * speed;
    }
}