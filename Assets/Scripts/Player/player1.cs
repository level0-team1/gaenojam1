using UnityEngine;

public class Player1Move : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animation anim;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animation>();
    }

    void Update()
    {
        moveInput = Vector2.zero;

        // 위
        if (Input.GetKey(KeyCode.W))
        {
            moveInput.y = 1;
            anim.Play("Player1_W_Up");
        }

        // 아래
        else if (Input.GetKey(KeyCode.S))
        {
            moveInput.y = -1;
            anim.Play("Player1_W_Down");
        }

        // 왼쪽
        else if (Input.GetKey(KeyCode.A))
        {
            moveInput.x = -1;
            anim.Play("Player1_W_Left");
        }

        // 오른쪽
        else if (Input.GetKey(KeyCode.D))
        {
            moveInput.x = 1;
            anim.Play("Player1_W_Right");
        }

        // 안 움직일 때 Idle
        else
        {
            anim.Play("Player1_Idle");
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput.normalized * speed;
    }
}