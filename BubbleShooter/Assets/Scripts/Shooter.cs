using UnityEngine;
using UnityEngine.InputSystem;

public class Shooter : MonoBehaviour
{
    public Transform aimPivot;
    public Transform firePoint;
    public GameObject bubblePrefab;
    public float shootSpeed = 20f;
    public float spawnDelay = 0.3f;
    public Color[] bubbleColors;

    private Bubble currentBubble;
    private bool canShoot = true;

    void Start()
    {
        if (bubblePrefab == null)
        {
            GridSystem grid = FindObjectOfType<GridSystem>();
            if (grid != null)
            {
                bubblePrefab = grid.bubblePrefab;
            }
        }

        if (bubbleColors == null || bubbleColors.Length == 0)
        {
            GridSystem grid = FindObjectOfType<GridSystem>();
            if (grid != null && grid.bubbleColors != null && grid.bubbleColors.Length > 0)
            {
                bubbleColors = grid.bubbleColors;
            }
            else
            {
                bubbleColors = new[] { Color.red, Color.green, Color.blue, Color.yellow };
            }
        }

        SpawnBubble();
    }

    void Update()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isGameOver || GameManager.Instance.isVictory))
        {
            canShoot = false;
            if (currentBubble != null)
            {
                Destroy(currentBubble.gameObject);
                currentBubble = null;
            }
            return;
        }

        Aim();

        if (WasShootPressedThisFrame() && canShoot)
        {
            Shoot();
        }
    }

    bool WasShootPressedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    void Aim()
    {
        if (aimPivot == null || Camera.main == null) return;

        Vector2 pointer = Vector2.zero;
        if (Touchscreen.current != null)
        {
            pointer = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        else if (Mouse.current != null)
        {
            pointer = Mouse.current.position.ReadValue();
        }

        Vector3 mouse = pointer;
        mouse.z = Mathf.Abs(Camera.main.transform.position.z);
        mouse = Camera.main.ScreenToWorldPoint(mouse);

        Vector2 dir = mouse - aimPivot.position;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        angle = Mathf.Clamp(angle, 10f, 170f);

        aimPivot.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Shoot()
    {
        if (currentBubble == null || firePoint == null || !currentBubble.gameObject.activeInHierarchy) return;

        canShoot = false;

        Vector2 dir = firePoint.right.normalized;
        float speed = Mathf.Clamp(shootSpeed, 3f, 20f);
        currentBubble.Shoot(dir, speed);

        GameManager.Instance?.OnBubbleShot();

        currentBubble = null;

        Invoke(nameof(SpawnBubble), spawnDelay);
    }

    void SpawnBubble()
    {
        if (GameManager.Instance != null && (GameManager.Instance.isGameOver || GameManager.Instance.isVictory))
        {
            canShoot = false;
            return;
        }

        if (bubblePrefab == null || firePoint == null)
        {
            Debug.LogWarning("Shooter: Missing bubblePrefab or firePoint, cannot spawn shooter bubble.");
            return;
        }

        if (currentBubble != null)
        {
            Destroy(currentBubble.gameObject);
            currentBubble = null;
        }

        GameObject go = Instantiate(bubblePrefab, firePoint.position, Quaternion.identity);
        Bubble bubble = go.GetComponent<Bubble>();

        if (bubble != null)
        {
            bubble.PrepareInLauncher(firePoint.position);
            int colorId = GetRandomColorId();
            bubble.SetColor(colorId, bubbleColors[Mathf.Clamp(colorId, 0, bubbleColors.Length - 1)]);
            currentBubble = bubble;
            canShoot = true;
        }
        else
        {
            Destroy(go);
            canShoot = false;
        }
    }

    int GetRandomColorId()
    {
        return Random.Range(0, bubbleColors.Length);
    }
}