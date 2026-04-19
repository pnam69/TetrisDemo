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
    [SerializeField] private float popDuration = 0.12f;
    private bool isPopping;

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

        grid = Object.FindFirstObjectByType<GridSystem>();

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

    // attachTo: optional transform to parent the bubble to (e.g., the firePoint)
    public void PrepareInLauncher(Vector3 spawnPos, Transform attachTo = null)
    {
        isSnapped = false;

        if (attachTo != null)
        {
            transform.SetParent(attachTo, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        else
        {
            transform.SetParent(null);
            transform.position = spawnPos;
        }

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

    public void DropAndFall()
    {
        if (rb == null) return;

        isSnapped = false;
        transform.SetParent(null, true);

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;
        rb.gravityScale = 1f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        if (circleCollider != null)
            circleCollider.isTrigger = true;

        // ensure falling clones eventually clean up
        StartCoroutine(AutoDestroyAfterFall());
    }

    System.Collections.IEnumerator AutoDestroyAfterFall()
    {
        // wait up to 6 seconds then destroy if still present
        float timeout = 6f;
        float t = 0f;
        while (t < timeout)
        {
            if (transform.position.y < CleanupY)
                break;
            t += Time.deltaTime;
            yield return null;
        }

        if (gameObject != null)
            Destroy(gameObject);
    }

    public void PopAndDestroy()
    {
        if (isPopping)
            return;

        StartCoroutine(PopRoutine());
    }

    System.Collections.IEnumerator PopRoutine()
    {
        isPopping = true;

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        if (circleCollider != null)
            circleCollider.enabled = false;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 fromScale = transform.localScale;
        Color fromColor = sr != null ? sr.color : Color.white;
        float t = 0f;

        while (t < popDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popDuration);
            transform.localScale = Vector3.Lerp(fromScale, fromScale * 1.25f, k);

            if (sr != null)
            {
                Color c = fromColor;
                c.a = Mathf.Lerp(fromColor.a, 0f, k);
                sr.color = c;
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if (isSnapped) return;
        if (rb == null) return;
        if (col.contactCount == 0) return;

        Vector2 normal = col.contacts[0].normal;
        Vector2 hitPoint = col.contacts[0].point;

        if (col.gameObject.CompareTag("Wall"))
        {
            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal);
            return;
        }

        bool hitBubble = col.collider.GetComponent<Bubble>() != null;
        bool hitTop = normal.y < -0.3f;

        if (!hitBubble && !hitTop) return;

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
            grid = Object.FindFirstObjectByType<GridSystem>();

        if (grid != null)
            grid.RequestSnap(this, anchor, hitPoint);
    }
}