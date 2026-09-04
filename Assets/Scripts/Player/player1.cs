using UnityEngine;

public class Player1Move : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Animator    anim;
    private Vector2     moveInput;
    private Inventory   inv;
    private PlayerStatus status;

    void Start()
    {
        rb     = GetComponent<Rigidbody2D>();
        anim   = GetComponent<Animator>();
        inv    = GetComponent<Inventory>();
        status = GetComponent<PlayerStatus>();
    }

    void Update()
    {
        if (GamePhaseManager.Instance.currentPhase != GamePhase.Farming || inv.isUIOpen
            || (status != null && status.IsStunned))
        {
            moveInput = Vector2.zero;
            anim.SetFloat("Speed", 0);
            return;
        }

        moveInput.x = 0;
        moveInput.y = 0;

        if (Input.GetKey(KeyCode.W)) moveInput.y =  1;
        if (Input.GetKey(KeyCode.S)) moveInput.y = -1;
        if (Input.GetKey(KeyCode.A)) moveInput.x = -1;
        if (Input.GetKey(KeyCode.D)) moveInput.x =  1;

        if (status != null && status.IsReversed) moveInput = -moveInput;

        if (moveInput != Vector2.zero)
        {
            anim.SetFloat("Dirx", moveInput.x);
            anim.SetFloat("Diry", moveInput.y);
        }
        anim.SetFloat("Speed", moveInput.magnitude);
    }

    void FixedUpdate()
    {
        float multiplier = (status != null) ? status.SpeedMultiplier : 1f;
        rb.linearVelocity = moveInput.normalized * speed * multiplier;
    }
}
