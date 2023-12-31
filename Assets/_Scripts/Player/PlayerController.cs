using System;
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

    // Tap for jump
    private float lastTapTime = 0f;
    private int tapCount = 0;
    private float doubleTapTime = 0.2f; // Time window for double tap (in seconds)


    [Header("Swipe Settings")]
    [SerializeField] private float horizontalSwipeThreshold = 5f;
    private float verticalSwipeThreshold = 3f;
    private bool isSwiping = false;

    [Header("Keyboard Settings")]
    [SerializeField] private float sideMoveSpeed = 2f; // Speed for left and right movement

    [SerializeField] private GameManager gameManager;

    public int Coins { get; set; }

    // State
    private bool canJump = true;
    private Vector2 touchStart;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        forwardSpeed = startSpeed; // initialize with start speed
    }

    private void Start()
    {
        Coins = 0;
    }

    private void Update()
    {
        HandleJump();
        HandleSwipe();
        HandleKeyboardInput();
        HandleFallOffTrack();
    }

    private void FixedUpdate()
    {
        HandleMovement();
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
        else if ((Input.GetButtonDown("Jump") /*|| Input.GetButtonDown("Fire1")*/) && canJump)
        {
            Jump();
        }
    }

    private void HandleFallOffTrack() 
    {
        if (gameObject.transform.position.y < -1.0) 
        {
            Destroy(gameObject);
            gameManager.SetGameOverActive();
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

    private float lateralMovementSpeed = 5.0f; // Adjust this value as needed for your game

    private void MoveLeft()
    {
        // Calculate the amount to move
        float moveAmount = lateralMovementSpeed * Time.deltaTime;

        // Move the object to the left
        Vector3 newPosition = rb.position - new Vector3(moveAmount, 0, 0);

        // Apply the new position
        rb.MovePosition(newPosition);
    }

    private void MoveRight()
    {
        // Calculate the amount to move
        float moveAmount = lateralMovementSpeed * Time.deltaTime;

        // Move the object to the right
        Vector3 newPosition = rb.position + new Vector3(moveAmount, 0, 0);

        // Apply the new position
        rb.MovePosition(newPosition);
    }

    private void HandleSwipe()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 swipeDelta;
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    // double tap
                    if ((Time.time - lastTapTime) < doubleTapTime)
                    {
                        tapCount++;
                    }
                    else
                    {
                        tapCount = 1;
                    }
                    lastTapTime = Time.time;

                    //if (tapCount == 2 && canJump)
                    //{
                    //    Jump();
                    //    tapCount = 0; // Reset tap count after a double tap
                    //}

                    touchStart = touch.position;
                    isSwiping = true;
                    break;

                case TouchPhase.Moved:
                    if (isSwiping)
                    {
                        swipeDelta = touch.position - touchStart;

                        // Check if the swipe is vertical
                        if (Mathf.Abs(swipeDelta.x) > horizontalSwipeThreshold && Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                        {
                            // Update position based on swipeDelta
                            if (swipeDelta.x < 0)
                            {
                                MoveLeft();
                            }
                            else if (swipeDelta.x > 0)
                            {
                                MoveRight();
                            }
                        }
                    }
                    break;

                case TouchPhase.Ended:
                    swipeDelta = touch.position - touchStart;
                    
                    // Check if the swipe is vertical
                    if (Mathf.Abs(swipeDelta.y) > verticalSwipeThreshold && Mathf.Abs(swipeDelta.y) > Mathf.Abs(swipeDelta.x))
                    {
                        // Check if the swipe is upwards
                        if (swipeDelta.y > 0 && canJump)
                        {
                            Jump();
                        }
                    }

                    // Update the start position for the next frame
                    touchStart = touch.position;
                    break;

                case TouchPhase.Canceled:
                    isSwiping = false;
                    break;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<Track>())
        {
            canJump = true;
        }

        if (collision.gameObject.GetComponent<StaticObstacle>() || collision.gameObject.GetComponent<RotatingBarrier>()) 
        {
            Destroy(gameObject);
            gameManager.SetGameOverActive();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.activeInHierarchy && other.gameObject.GetComponent<CollectibleRotator>())
        {
            Coins += 1;
            other.gameObject.SetActive(false);
        }
    }
}