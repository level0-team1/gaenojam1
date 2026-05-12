using UnityEngine;

public class Player1Move : MonoBehaviour
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

        if (Input.GetKey(KeyCode.W))
        {
            moveInput.y = 1;
        }

        if (Input.GetKey(KeyCode.S))
        {
            moveInput.y = -1;
        }

        if (Input.GetKey(KeyCode.A))
        {
            moveInput.x = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            moveInput.x = 1;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity =
            moveInput.normalized * speed;
    }
}