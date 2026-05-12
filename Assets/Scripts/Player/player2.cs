using UnityEngine;

public class Player2Move : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput.x = 0;
        moveInput.y = 0;

        // 위 방향키
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveInput.y = 1;
        }

        // 아래 방향키
        if (Input.GetKey(KeyCode.DownArrow))
        {
            moveInput.y = -1;
        }

        // 왼쪽 방향키
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveInput.x = -1;
        }

        // 오른쪽 방향키
        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveInput.x = 1;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * speed;
    }
}