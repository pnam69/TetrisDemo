using UnityEngine;

public class Bubble : MonoBehaviour
{
    public int colorID;

    [HideInInspector] public Vector2Int gridPos;
    [HideInInspector] public bool isSnapped;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Shoot(Vector2 dir, float speed)
    {
        rb.linearVelocity = dir.normalized * speed;
    }

    public void StopAndLock()
    {
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        isSnapped = true;
    }
    void OnCollisionEnter2D(Collision2D col)
    {
        if (isSnapped) return;

        if (col.gameObject.CompareTag("Bubble") ||
            col.gameObject.CompareTag("Top"))
        {
            GridSystem grid = FindObjectOfType<GridSystem>();
            grid.SnapBubble(this);
        }

        if (col.gameObject.CompareTag("Wall"))
        {
            Vector2 normal = col.contacts[0].normal;
            rb.linearVelocity = Vector2.Reflect(rb.linearVelocity, normal);
        }
    }
}