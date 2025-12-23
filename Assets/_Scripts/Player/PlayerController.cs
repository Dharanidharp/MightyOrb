using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // ADDED: An enum to make control types clear
    public enum ControlType { Swipe, Tilt }
    private ControlType currentControlScheme;

    // Event now passes current Coins count. Streak logic is centralized in GameManager.
    public event Action<int, Vector3> OnCoinCollected; // Modified to include position

    [Header("Movement Settings")]
    [SerializeField] private float startSpeed = 5f;
    [SerializeField] private float acceleration = 0.05f;
    [SerializeField] private float maxSpeed = 15f;
    [SerializeField] private float lateralForce = 75f;
    [SerializeField] private float tiltSensitivity = 2.0f;
    private float forwardSpeed;
    public float CurrentForwardSpeed => forwardSpeed;

    [Header("Jump Settings")]
    [SerializeField] private float jumpForce = 5f;

    [Header("Gesture Settings")]
    [SerializeField] private float minSwipeDistance = 100f; // For jump
    [SerializeField] private float maxSwipeTime = 1.0f;    // For jump
    [SerializeField] private float swipeSensitivity = 0.1f;  // ADDED: For swipe-drag sensitivity

    [Header("References")]
    [SerializeField] private GameManager gameManager;

    public int Coins { get; private set; }

    private Rigidbody rb;
    private bool isGrounded = true;
    private float horizontalInput = 0f; // CHANGED: This is now the master input value
    private bool jumpQueued = false;

    private Vector2 touchStartPos;
    private float touchStartTime;

    // ADDED: For swipe-drag controls
    private Vector2 swipeDragStartPos;
    private bool isDragging = false;

    // --- Power-Up State ---
    public bool IsInvincible { get; private set; }
    private Coroutine currentPowerUpCoroutine; // To track the active power-up
    private PowerUp activePowerUp;
    // -----------------------------

    // --- ADDED: Coin Magnet State ---
    public bool IsCoinMagnetActive { get; private set; }
    private float currentMagnetRadius;
    private float currentMagnetStrength;
    private Collider[] coinBuffer = new Collider[50]; // Buffer for Physics.OverlapSphereNonAlloc
    // ---------------------------------

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        forwardSpeed = startSpeed;

        // --- ADDED: Load the control scheme setting ---
        // We load the saved setting. 0 = Swipe (default), 1 = Tilt.
        // NOTE: Make sure your SettingsMenu script is in your Main Menu scene
        // and uses this "UseTiltControls" key.
        int controlPref = PlayerPrefs.GetInt("UseTiltControls", 0);
        currentControlScheme = (ControlType)controlPref;
        Debug.Log("Current control scheme loaded: " + currentControlScheme);
        // ---------------------------------------------

        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager not found in scene!");
        }
    }

    private void Start()
    {
        Coins = 0;
        IsInvincible = false;
        IsCoinMagnetActive = false; // Initialize magnet state
    }

    private void Update()
    {
        if (gameManager.IsGameOver)
        {
            horizontalInput = 0;
            return;
        }

        // --- RE-STRUCTURED: Input Handling ---
        // 1. Reset horizontal input each frame
        horizontalInput = 0f;

        // 2. Always handle keyboard (for editor testing)
        HandleKeyboardInput();

        // 3. Handle the selected mobile scheme
        HandleMobileInput();

        // 4. Handle non-physics logic
        HandleFallOffTrack();
        // -------------------------------------
    }

    private void FixedUpdate()
    {
        if (gameManager.IsGameOver)
        {
            rb.velocity = Vector3.zero;
            return;
        }

        HandleMovement();
        HandleJump();

        if (IsCoinMagnetActive)
        {
            PullCoins();
        }
    }

    // --- NEW: Handles only Keyboard input ---
    private void HandleKeyboardInput()
    {
        // Use GetAxis for smooth input in editor
        float keyboardInput = Input.GetAxis("Horizontal"); // Changed from GetAxisRaw for smoother editor controls

        // Only apply if keyboard is being used
        if (Mathf.Abs(keyboardInput) > 0.1f)
        {
            horizontalInput = keyboardInput;
        }

        // Keyboard jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpQueued = true;
        }
    }

    // --- NEW: Handles all mobile input (Tilt or Swipe) ---
    private void HandleMobileInput()
    {
        // --- A: TILT CONTROLS ---
        if (currentControlScheme == ControlType.Tilt)
        {
            float tiltInput = Input.acceleration.x * tiltSensitivity;
            // Only apply tilt if it's significant and no keyboard input
            if (Mathf.Abs(tiltInput) > 0.1f && Mathf.Abs(horizontalInput) < 0.1f)
            {
                horizontalInput = Mathf.Clamp(tiltInput, -1f, 1f);
            }
        }
        // --- B: SWIPE-DRAG CONTROLS ---
        else // (currentControlScheme == ControlType.Swipe)
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    // Check if this is a swipe-up for jump or a horizontal drag
                    touchStartPos = touch.position; // For jump check
                    touchStartTime = Time.time;    // For jump check

                    swipeDragStartPos = touch.position; // For drag check
                    isDragging = false; // Don't assume dragging yet
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    // Check horizontal distance moved
                    float deltaX = touch.position.x - swipeDragStartPos.x;
                    // Check vertical distance moved
                    float deltaY = touch.position.y - swipeDragStartPos.y;

                    // If it's *mostly* horizontal, start dragging
                    if (Mathf.Abs(deltaX) > 10f && Mathf.Abs(deltaX) > Mathf.Abs(deltaY)) // 10px threshold to start drag
                    {
                        isDragging = true;
                    }

                    if (isDragging)
                    {
                        // Calculate horizontal drag amount based on *current* position vs start
                        float dragAmount = touch.position.x - swipeDragStartPos.x;
                        // Convert pixel drag into a -1 to 1 input value
                        horizontalInput = Mathf.Clamp(dragAmount * swipeSensitivity, -1f, 1f);
                    }
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    // Check if this was a jump swipe (and not a horizontal drag)
                    if (!isDragging)
                    {
                        float swipeDuration = Time.time - touchStartTime;
                        float swipeDistanceY = touch.position.y - touchStartPos.y;
                        float swipeDistanceX = touch.position.x - touchStartPos.x;

                        if (swipeDuration < maxSwipeTime &&
                            swipeDistanceY > minSwipeDistance &&
                            Mathf.Abs(swipeDistanceY) > Mathf.Abs(swipeDistanceX)) // It's a vertical swipe
                        {
                            if (isGrounded)
                            {
                                jumpQueued = true;
                            }
                        }
                    }

                    // Reset drag state on touch end
                    isDragging = false;
                }
            }
        }

        // --- C: TILT MODE JUMP (Swipe Up) ---
        // We must *also* check for jump swipes in TILT mode,
        // as the swipe logic above is only in the 'else' block.
        if (currentControlScheme == ControlType.Tilt && Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                touchStartPos = touch.position;
                touchStartTime = Time.time;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                float swipeDuration = Time.time - touchStartTime;
                float swipeDistanceY = touch.position.y - touchStartPos.y;
                float swipeDistanceX = touch.position.x - touchStartPos.x;

                if (swipeDuration < maxSwipeTime &&
                    swipeDistanceY > minSwipeDistance &&
                    Mathf.Abs(swipeDistanceY) > Mathf.Abs(swipeDistanceX)) // It's a vertical swipe
                {
                    if (isGrounded)
                    {
                        jumpQueued = true;
                    }
                }
            }
        }
    }

    private void HandleMovement()
    {
        forwardSpeed = Mathf.Clamp(forwardSpeed + acceleration * Time.fixedDeltaTime, startSpeed, maxSpeed);
        Vector3 forwardVelocity = new Vector3(0, 0, forwardSpeed);
        Vector3 deltaV = forwardVelocity - new Vector3(0, 0, rb.velocity.z);
        rb.AddForce(deltaV, ForceMode.VelocityChange);

        // This line is now simpler.
        // It just uses the final 'horizontalInput' value,
        // regardless of where it came from (keyboard, tilt, or swipe).
        float horizontalForce = horizontalInput * lateralForce;
        rb.AddForce(new Vector3(horizontalForce, 0, 0), ForceMode.Acceleration);
    }

    private void HandleJump()
    {
        if (jumpQueued)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpQueued = false;
            isGrounded = false;

            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayJumpSFX();
            }
        }
    }

    private void HandleFallOffTrack()
    {
        if (gameObject.transform.position.y < -1.0)
        {
            gameManager.PlayerDied();
            // gameObject.SetActive(false); // DO NOT DO THIS

            // Hitting this check means the player is falling,
            // so we need to disable the Rigidbody
            // to stop it from spamming the 'PlayerDied' call.
            rb.isKinematic = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<Track>())
        {
            isGrounded = true;
            SoundManager.Instance.PlayLandSFX();
        }

        Obstacle obstacle = collision.gameObject.GetComponent<Obstacle>();

        if (obstacle != null)
        {
            if (IsInvincible)
            {
                Destroy(collision.gameObject);
            }
            else
            {
                gameManager.PlayerDied(); // Call the new death method
                                          // gameObject.SetActive(false); // DO NOT DO THIS
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check for coins
        // This is where a coin "officially" gets collected
        if (other.gameObject.activeInHierarchy && other.gameObject.GetComponent<CollectibleRotator>())
        {
            CollectCoin(other.gameObject);
        }
    }

    private void CollectCoin(GameObject coinObject)
    {
        Coins += 1;
        Vector3 coinPosition = coinObject.transform.position;
        coinObject.SetActive(false); // Deactivate the coin
        OnCoinCollected?.Invoke(Coins, coinPosition);
    }
    // ----------------------------------------------

    public void IncreaseForwardSpeed(float amount)
    {
        forwardSpeed = Mathf.Min(forwardSpeed + amount, maxSpeed);
    }

    // --- Power-Up Methods ---

    public void SetInvincibility(bool state)
    {
        IsInvincible = state;
        Debug.Log("Player Invincibility = " + state);
    }

    public void SetCoinMagnet(bool state, float radius, float strength)
    {
        IsCoinMagnetActive = state;
        currentMagnetRadius = radius;
        currentMagnetStrength = strength;
    }

    public void StartPowerUp(PowerUp powerUp)
    {
        if (currentPowerUpCoroutine != null && activePowerUp != null)
        {
            activePowerUp.Deactivate(this);
            StopCoroutine(currentPowerUpCoroutine);
        }

        activePowerUp = powerUp;
        powerUp.Activate(this);
        currentPowerUpCoroutine = StartCoroutine(PowerUpCooldownRoutine(powerUp));
    }

    private IEnumerator PowerUpCooldownRoutine(PowerUp powerUp)
    {
        yield return new WaitForSeconds(powerUp.duration);

        if (activePowerUp == powerUp)
        {
            powerUp.Deactivate(this);
            activePowerUp = null;
            currentPowerUpCoroutine = null;
        }
    }

    private void PullCoins()
    {
        int numColliders = Physics.OverlapSphereNonAlloc(transform.position, currentMagnetRadius, coinBuffer, LayerMask.GetMask("Coin"));

        for (int i = 0; i < numColliders; i++)
        {
            Collider coinCollider = coinBuffer[i];

            if (coinCollider != null && coinCollider.gameObject.activeInHierarchy && coinCollider.GetComponent<CollectibleRotator>() != null)
            {
                float distance = Vector3.Distance(transform.position, coinCollider.transform.position);

                if (distance < 0.5f)
                {
                    CollectCoin(coinCollider.gameObject);
                }
                else
                {
                    Vector3 directionToPlayer = (transform.position - coinCollider.transform.position).normalized;
                    coinCollider.transform.Translate(directionToPlayer * currentMagnetStrength * Time.fixedDeltaTime, Space.World);
                }
            }
        }
    }

    public void RevivePlayer()
    {
        // 1. Re-enable Rigidbody physics if it was disabled by fall
        rb.isKinematic = false;

        // 2. Reset physics state
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 3. Move player to a safe position
        // We keep their Z progress, but reset X and move them up
        transform.position = new Vector3(0, 2f, transform.position.z);

        // 4. Reset internal states
        isGrounded = true;
        jumpQueued = false;
        horizontalInput = 0f;

        // Optional: Give a few seconds of invincibility
        // StartPowerUp(myInvincibilityPowerUpAsset);
    }
}