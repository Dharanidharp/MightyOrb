using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // Player Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float acceleration = 0.05f;
    [SerializeField] private float maxSpeed = 15f;
    private float forwardSpeed;

    [Header("Jump Settings")]
    [SerializeField] private AnimationCurve jumpCurve;
    [SerializeField] private float jumpForce = 5f;
    private bool isJumping = false;
    private float jumpTime = 0f;

    [Header("Swipe Settings")]
    [SerializeField] private float swipeThreshold = 50f;

    [Header("Keyboard Settings")]
    [SerializeField] private float sideMoveSpeed = 2f; // Speed for left and right movement

    // State
    private bool canJump = true;
    private Vector2 touchStart;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        forwardSpeed = startSpeed; // initialize with start speed
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleSwipe();
        HandleKeyboardInput();
    }

    private void HandleMovement()
    {
        // Increase speed gradually
        forwardSpeed = Mathf.Clamp(forwardSpeed + acceleration * Time.deltaTime, startSpeed, maxSpeed);

        // Move the orb forward
        Vector3 moveDirection = new Vector3(0, rb.velocity.y, forwardSpeed);
        rb.velocity = moveDirection;
    }

    private void HandleJump()
    {
        // Smooth Jump using AnimationCurve
        if (isJumping)
        {
            jumpTime += Time.deltaTime;
            float height = jumpCurve.Evaluate(jumpTime);
            Vector3 position = transform.position;
            position.y = height;
            transform.position = position;

            if (jumpTime >= jumpCurve.keys[jumpCurve.length - 1].time)
            {
                isJumping = false;
                jumpTime = 0f;
            }
        }
        else if (Input.GetButtonDown("Jump") && canJump)
        {
            Jump();
        }
    }

    private void Jump()
    {
        // Apply an upward force
        Vector3 jumpVector = new Vector3(0, jumpForce, 0);
        rb.AddForce(jumpVector, ForceMode.VelocityChange);
        canJump = false;
    }

    private void HandleKeyboardInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        // Move left or right based on keyboard input
        Vector3 sideMovement = new Vector3(horizontalInput * sideMoveSpeed, 0, 0);
        transform.position += sideMovement * Time.deltaTime;
    }

    private void HandleSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = touch.position;
                    break;

                case TouchPhase.Ended:
                    Vector2 swipeDelta = touch.position - touchStart;

                    if (Mathf.Abs(swipeDelta.x) > swipeThreshold)
                    {
                        if (swipeDelta.x < 0)
                        {
                            MoveLeft();
                        }
                        else
                        {
                            MoveRight();
                        }
                    }
                    break;
            }
        }
    }

    private void MoveLeft()
    {
        // Implement lane switching to the left
    }

    private void MoveRight()
    {
        // Implement lane switching to the right
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            canJump = true;
        }

        if (collision.gameObject.GetComponent<StaticObstacle>()) 
        {
            Destroy(gameObject);
        }
    }
}