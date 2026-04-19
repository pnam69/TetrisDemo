using UnityEngine;
using UnityEngine.InputSystem;

public class Dinosaur : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Run,
        Jump,
        Fall,
        Slide,
        Dead
    }

    [Header("References")]
    public Rigidbody2D rb;
    public Animator animator;
    public Collider2D mainCollider;
    public Collider2D slideCollider;

    [Header("Jump")]
    public float jumpForce = 12f;
    public float extraJumpForce = 8f;
    public float fallMultiplier = 4.0f;
    public float lowJumpMultiplier = 2f;
    public float maxJumpHoldTime = 0.2f;
    public bool enableDoubleJump = false;
    public int maxJumpCount = 2;
    private float jumpHoldTimer;
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Slide")]
    public float slideDuration = 1.2f;

    private PlayerState currentState = PlayerState.Idle;

    private bool isGrounded;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool slidePressed;
    private int jumpCount;

    private float slideTimer;

    void Update()
    {
        // If there's no GameManager or the game hasn't started yet, stay idle on the start screen
        if (GameManager.Instance == null || !GameManager.Instance.gameStarted)
        {
            currentState = PlayerState.Idle;
            UpdateAnimation();
            return;
        }

        // If there's a UI start panel visible, keep the player idle (defensive for cases where gameStarted may already be true)
        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        if (ui != null && ui.startPanel != null && ui.startPanel.activeInHierarchy)
        {
            currentState = PlayerState.Idle;
            UpdateAnimation();
            return;
        }

        HandleInput();
        UpdateState();
        CheckGround();
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        ApplyPhysics();
    }

    // ---------------- INPUT ----------------
    Vector2 startTouchPos;

    void HandleInput()
    {
        jumpPressed = false;
        slidePressed = false;

        // -------- PC INPUT --------
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpPressed = true;
                jumpHeld = true;
            }

            if (Keyboard.current.spaceKey.isPressed)
            {
                jumpHeld = true;
            }

            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
            {
                jumpHeld = false;
            }

            if (Keyboard.current.sKey.wasPressedThisFrame)
            {
                slidePressed = true;
            }
        }

        // -------- MOBILE INPUT (wonÅft affect PC) --------
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                startTouchPos = touch.position.ReadValue();
                jumpPressed = true;
                jumpHeld = true;
            }

            if (touch.press.isPressed)
            {
                jumpHeld = true;

                Vector2 currentPos = touch.position.ReadValue();
                if (currentPos.y < startTouchPos.y - 80f)
                {
                    slidePressed = true;
                }
            }

            if (touch.press.wasReleasedThisFrame)
            {
                jumpHeld = false;
            }
        }
    }

    // ---------------- STATE ----------------
    void UpdateState()
    {
        if (currentState == PlayerState.Dead)
            return;
        // If the game hasn't started yet, stay idle ? defensive guard
        if (GameManager.Instance == null || !GameManager.Instance.gameStarted)
        {
            currentState = PlayerState.Idle;
            return;
        }
        if (slidePressed && isGrounded)
        {
            StartSlide();
            return;
        }

        bool canAirJump = enableDoubleJump && jumpCount < maxJumpCount;
        if (jumpPressed && (isGrounded || canAirJump))
        {
            Jump();
            return;
        }

        if (!isGrounded)
        {
            if (rb.linearVelocity.y > 0.1f)
                currentState = PlayerState.Jump;
            else
                currentState = PlayerState.Fall;
        }
        else
        {
            if (currentState != PlayerState.Slide)
                currentState = PlayerState.Run;
        }

        jumpPressed = false;
        slidePressed = false;
    }

    // ---------------- ACTIONS ----------------
    void Jump()
    {
        jumpCount++;
        rb.linearVelocity = new Vector2(0, jumpForce);
        jumpHoldTimer = maxJumpHoldTime;
        currentState = PlayerState.Jump;
        jumpPressed = false;
    }

    void StartSlide()
    {
        currentState = PlayerState.Slide;
        slideTimer = slideDuration;

        mainCollider.enabled = false;
        slideCollider.enabled = true;

        slidePressed = false;
    }

    void EndSlide()
    {
        mainCollider.enabled = true;
        slideCollider.enabled = false;

        currentState = PlayerState.Run;
    }

    // ---------------- PHYSICS ----------------
    void ApplyPhysics()
    {
        // Lock X position (important)
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Hold jump
        if (jumpHeld && jumpHoldTimer > 0)
        {
            rb.linearVelocity += Vector2.up * extraJumpForce * Time.fixedDeltaTime;
            jumpHoldTimer -= Time.fixedDeltaTime;
        }

        // Better fall
        if (rb.linearVelocity.y <= 0)
        {
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            rb.gravityScale = lowJumpMultiplier;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        // Slide timer
        if (currentState == PlayerState.Slide)
        {
            slideTimer -= Time.fixedDeltaTime;
            if (slideTimer <= 0)
                EndSlide();
        }
    }

    void CheckGround()
    {
        Collider2D hit;

        if (groundLayer.value == 0)
        {
            hit = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius
            );
        }
        else
        {
            hit = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        isGrounded = hit != null;
        if (isGrounded)
        {
            jumpCount = 0;
        }

    }

    void UpdateAnimation()
    {
        animator.SetInteger("State", (int)currentState);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Obstacle"))
        {
            Die();
        }
    }
    void OnDrawGizmos()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
    void Die()
    {
        currentState = PlayerState.Dead;
        rb.linearVelocity = Vector2.zero;
        GameManager.Instance.GameOver();
    }
}