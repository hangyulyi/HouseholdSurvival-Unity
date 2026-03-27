using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    // Movement boundary
    [SerializeField] private bool useBounds = true;
    [SerializeField] private float boundsMinX = -8f;
    [SerializeField] private float boundsMaxX = 8f;
    [SerializeField] private float boundsMinY = -4.5f;
    [SerializeField] private float boundsMaxY = 4.5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 targetPos;
    private bool isClickMoving = false;
    private int facingDirection = 1;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        targetPos = transform.position; // Initialize target at current position
    }

    void Update()
    {

        // Click-to-Move Logic
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector3 mousePos = Mouse.current.position.ReadValue();
            mousePos.z = Mathf.Abs(Camera.main.transform.position.z);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

            if (!useBounds || (worldPos.x >= boundsMinX && worldPos.x <= boundsMaxX && worldPos.y >= boundsMinY && worldPos.y <= boundsMaxY))
            {
                targetPos = new Vector2(Mathf.Clamp(worldPos.x, boundsMinX, boundsMaxX), Mathf.Clamp(worldPos.y, boundsMinY, boundsMaxY));
                isClickMoving = true;
            }
            
        }

        // Determine Movement Direction
        Vector2 currentPos = transform.position;
        if (isClickMoving)
        {
            // Calculate direction toward target
            Vector2 dir = (targetPos - currentPos).normalized;
            moveInput = dir;

            if (Vector2.Distance(currentPos, targetPos) < 0.1f)
            {
                isClickMoving = false;
                moveInput = Vector2.zero;
            }
        }

        // Apply Velocity
        rb.linearVelocity = moveInput * moveSpeed;

        // Handle Animations & Flipping
        UpdateAnimations();
    }

    // WASD/Joystick movement
    public void Move(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        if (context.performed)
        {
            isClickMoving = false;
        }
    }

    private void UpdateAnimations()
    {
        bool isWalking = moveInput.magnitude > 0.01f;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            animator.SetFloat("InputX", moveInput.x);
            animator.SetFloat("InputY", moveInput.y);
            animator.SetFloat("LastInputX", moveInput.x);
            animator.SetFloat("LastInputY", moveInput.y);

            // Flip logic
            if (moveInput.x > 0.1f && facingDirection < 0 || moveInput.x < -0.1f && facingDirection > 0)
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        facingDirection *= -1;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}