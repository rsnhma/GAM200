using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Stats Reference")]
    public PlayerStats playerStats;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool canMove = true;
    private bool isSprinting;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!canMove)
        {
            movement = Vector2.zero;
            return;
        }

        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
        movement = movement.normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift);
    }

    void FixedUpdate()
    {
        float speed = isSprinting ? playerStats.sprintSpeed : playerStats.walkSpeed;
        rb.linearVelocity = movement * speed;
    }

    public void FreezeMovement()
    {
        canMove = false;
        movement = Vector2.zero;
    }

    public void UnfreezeMovement()
    {
        canMove = true;
    }
}