using UnityEngine;

public class Bubble : MonoBehaviour
{
    public int colorID;

    [HideInInspector] public Vector2Int gridPos;
    [HideInInspector] public bool isSnapped;

    private Rigidbody2D rb;
    private CircleCollider2D circleCollider;
    private GridSystem grid;
    private const float CleanupY = -8f;

    public Rigidbody2D RB => rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
            circleCollider = gameObject.AddComponent<CircleCollider2D>();

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
            circleCollider.radius = sr.sprite.bounds.extents.x;
        else
            circleCollider.radius = 0.1f;

        circleCollider.isTrigger = false;

        grid = FindObjectOfType<GridSystem>();

        rb.freezeRotation = true;

        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void SetColor(int id, Color color)
    {
        colorID = id;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;
    }

    void Update()
    {
        if (isSnapped) return;

        if (transform.position.y < CleanupY)
        {
            Destroy(gameObject);
        }
    }

    public void PrepareInLauncher(Vector3 spawnPos)
    {
        isSnapped = false;

        transform.SetParent(null);
        transform.position = spawnPos;

        if (rb == null) return;
        rb.simulated = false;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    public void Shoot(Vector2 dir, float speed)
    {
        isSnapped = false;
        transform.SetParent(null, true);
        if (rb == null) return;
        rb.simulated = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.linearVelocity = dir.normalized * speed;
    }

    public void StopAndLock()
    {
        if (rb == null) return;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Static;
        rb.simulated = true;
        isSnapped = true;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isSnapped) return;
        if (rb == null) return;
        if (col.contactCount == 0) return;

        Vector2 normal = col.contacts[0].normal;
        Vector2 hitPoint = col.contacts[0].point;

        // Only wall bounce.
        if (col.gameObject.CompareTag("Wall"))
        {
            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal);
            return;
        }

        bool hitBubble = col.collider.GetComponent<Bubble>() != null;
        bool hitTop = normal.y < -0.3f;

        if (!hitBubble && !hitTop) return;

        // Freeze projectile before grid snap request.
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Bubble other = col.collider.GetComponent<Bubble>();
        Vector2Int? anchor = null;
        if (other != null && other.isSnapped)
        {
            anchor = other.gridPos;
        }

        if (grid == null)
            grid = FindObjectOfType<GridSystem>();

        if (grid != null)
            grid.RequestSnap(this, anchor, hitPoint);
    }
}