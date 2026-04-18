using UnityEngine;

public class Bird : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb2d;
    
    [Header("Flight Settings")]
    public float flapStrength = 8.5f;
    public float maxFallSpeed = 6f;
    public float tiltSpeed = 3f;
    public float maxTiltAngle = 40f;
    public float minTiltAngle = -40f;
    
    [Header("Bounds")]
    public float deathZoneY = -6f;
    public float ceilingY = 6f;
    
    private LogicScript logic;
    private bool isAlive;
    private bool hasGameOverTriggered;
    private float defaultGravityScale;

    void Start()
    {
        defaultGravityScale = rb2d != null ? rb2d.gravityScale : 1f;

        if (rb2d != null)
        {
            rb2d.gravityScale = 0f;
            rb2d.linearVelocity = Vector2.zero;
        }

        isAlive = false;

        GameObject logicObject = GameObject.FindGameObjectWithTag("Logic");
        if (logicObject != null)
        {
            logic = logicObject.GetComponent<LogicScript>();
        }
        else
        {
            Debug.LogError("Logic GameObject not found! Make sure there's a GameObject tagged 'Logic'.");
        }
    }

    public void BeginPlay()
    {
        if (rb2d != null)
        {
            rb2d.gravityScale = defaultGravityScale;
            rb2d.linearVelocity = Vector2.zero;
        }

        hasGameOverTriggered = false;
        isAlive = true;
        transform.rotation = Quaternion.identity;
    }

    void Update()
    {
        if (!isAlive || logic == null || logic.IsGameOver()) return;
        if (GameManager.Instance.inputLocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            Flap();
        }
        HandleInput();
        UpdateRotation();
        CheckBounds();
        ClampVelocity();
    }

    void HandleInput()
    {
        bool tapInput = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        bool mouseInput = Input.GetMouseButtonDown(0);
        bool keyboardInput = Input.GetKeyDown(KeyCode.Space);

        if (tapInput || mouseInput || keyboardInput)
        {
            Flap();
        }
    }

    void Flap()
    {
        rb2d.linearVelocity = Vector2.up * flapStrength;
        
        // Play flap sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFlap();
        }
    }

    void UpdateRotation()
    {
        float velocity = rb2d.linearVelocity.y;
        float targetAngle = Mathf.Lerp(minTiltAngle, maxTiltAngle, (velocity + 10f) / 20f);
        targetAngle = Mathf.Clamp(targetAngle, minTiltAngle, maxTiltAngle);
        
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
    }

    void ClampVelocity()
    {
        if (rb2d.linearVelocity.y < -maxFallSpeed)
        {
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, -maxFallSpeed);
        }
    }

    void CheckBounds()
    {
        if (transform.position.y < deathZoneY || transform.position.y > ceilingY)
        {
            Die();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Die();
    }

    void Die()
    {
        if (hasGameOverTriggered) return;
        
        hasGameOverTriggered = true;
        isAlive = false;

        if (rb2d != null)
        {
            rb2d.gravityScale = defaultGravityScale;
        }
        
        // Play hit sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayHit();
        }
        
        if (logic != null)
        {
            logic.gameOver();
        }
    }
}
