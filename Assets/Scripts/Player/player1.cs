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
        anim = GetComponent<Animation>(); //anim은 애니메이션이라는 속성을 들고옴
    }

    void Update()
    {
        moveInput.x = 0;
        moveInput.y = 0;

        if (Input.GetKey(KeyCode.W))
        {
            anim.Play("Player1_W_Up");
            moveInput.y = 1;
        }
        else if (Input.GetKey(KeyCode.W))
        {
            anim.Play("New state 1");
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