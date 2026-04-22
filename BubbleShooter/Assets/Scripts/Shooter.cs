using UnityEngine;
using UnityEngine.EventSystems;

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
    private int nextColorId = -1;
    public LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryMaxBounces = 2;
    [SerializeField] private float trajectoryMaxDistance = 25f;

    void Start()
    {
        if (bubblePrefab == null)
        {
            GridSystem grid = Object.FindFirstObjectByType<GridSystem>();
            if (grid != null)
            {
                bubblePrefab = grid.bubblePrefab;
            }
        }

        if (bubbleColors == null || bubbleColors.Length == 0)
        {
            GridSystem grid = Object.FindFirstObjectByType<GridSystem>();
            if (grid != null && grid.bubbleColors != null && grid.bubbleColors.Length > 0)
            {
                bubbleColors = grid.bubbleColors;
            }
            else
            {
                bubbleColors = new[] { Color.red, Color.green, Color.blue, Color.yellow };
            }
        }

        if (nextColorId < 0)
            nextColorId = GetRandomColorId();
        EnsureTrajectoryRef();
        SpawnBubble();
    }

    void EnsureTrajectoryRef()
    {
        if (trajectoryLine != null) return;
        GameObject go = GameObject.Find("TrajectoryLine");
        if (go == null)
        {
            go = new GameObject("TrajectoryLine");
            go.transform.SetParent(transform, false);
            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 0;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;
            lr.numCapVertices = 4;
            trajectoryLine = lr;
        }
        else
        {
            trajectoryLine = go.GetComponent<LineRenderer>();
            if (trajectoryLine == null) trajectoryLine = go.AddComponent<LineRenderer>();
        }
        if (trajectoryLine != null) trajectoryLine.enabled = false;
    }

    void UpdateTrajectoryPreview(Vector2 dir)
    {
        if (trajectoryLine == null || firePoint == null) return;

        trajectoryLine.enabled = true;
        float radius = 0.15f;
        CircleCollider2D cc = currentBubble != null ? currentBubble.GetComponent<CircleCollider2D>() : null;
        if (cc != null) radius = Mathf.Max(0.05f, cc.radius * Mathf.Abs(currentBubble.transform.lossyScale.x));

        Vector3 origin = firePoint.position;
        Vector2 d = dir.normalized;
        float remain = trajectoryMaxDistance;

        var points = new System.Collections.Generic.List<Vector3> { origin };

        for (int i = 0; i <= trajectoryMaxBounces && remain > 0.01f; i++)
        {
            RaycastHit2D hit = Physics2D.CircleCast(origin, radius, d, remain);
            if (!hit.collider)
            {
                points.Add(origin + (Vector3)(d * remain));
                break;
            }

            points.Add(hit.point);

            if (hit.collider.CompareTag("Wall"))
            {
                // subtract the distance traveled to the hit
                float traveled = Vector2.Distance(origin, hit.point);
                remain -= traveled;

                // reflect direction around the surface normal
                d = Vector2.Reflect(d, hit.normal).normalized;

                float pushOut = radius + 0.01f;
                origin = new Vector3(hit.point.x, hit.point.y, origin.z) + (Vector3)(hit.normal * pushOut) + (Vector3)(d * 0.001f);
                continue;
            }

            break;
        }

        trajectoryLine.positionCount = points.Count;
        for (int i = 0; i < points.Count; i++) trajectoryLine.SetPosition(i, points[i]);
    }

    void HideTrajectory()
    {
        if (trajectoryLine == null) return;
        trajectoryLine.enabled = false;
        trajectoryLine.positionCount = 0;
    }

    public void ResetShooter()
    {
        if (currentBubble != null)
        {
            Destroy(currentBubble.gameObject);
            currentBubble = null;
        }

        CancelInvoke(nameof(SpawnBubble));

        canShoot = true;

        if (bubblePrefab == null)
        {
            GridSystem grid = Object.FindFirstObjectByType<GridSystem>();
            if (grid != null)
                bubblePrefab = grid.bubblePrefab;
        }

        if (bubbleColors == null || bubbleColors.Length == 0)
        {
            GridSystem grid = Object.FindFirstObjectByType<GridSystem>();
            if (grid != null && grid.bubbleColors != null && grid.bubbleColors.Length > 0)
            {
                bubbleColors = grid.bubbleColors;
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

        Vector2 pointer;
        if (TryGetAimPointer(out pointer))
        {
            AimAtScreenPoint(pointer);

            // compute world direction and update trajectory preview
            if (Camera.main != null && aimPivot != null)
            {
                Vector3 world = Camera.main.ScreenToWorldPoint(new Vector3(pointer.x, pointer.y, Mathf.Abs(Camera.main.transform.position.z)));
                Vector2 dir = (world - aimPivot.position);
                UpdateTrajectoryPreview(dir);
            }
        }
        else
        {
            HideTrajectory();
        }

        if (WasShootPressedThisFrame() && canShoot)
        {
            Shoot();
        }
    }

    bool TryGetAimPointer(out Vector2 pointer)
    {
        pointer = Vector2.zero;

        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                continue;
            pointer = t.position;
            return true;
        }

        pointer = Input.mousePosition;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return false;
        }
        return true;
    }

    bool WasShootPressedThisFrame()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);
            if (t.phase != UnityEngine.TouchPhase.Began)
                continue;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                continue;

            return true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return false;

            return true;
        }

        return false;
    }

    void AimAtScreenPoint(Vector2 screenPos)
    {
        if (aimPivot == null || Camera.main == null) return;

        Vector3 mouse = screenPos;
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
            bubble.PrepareInLauncher(firePoint.position, firePoint);
            if (nextColorId < 0) nextColorId = GetRandomColorId();
            int colorId = nextColorId;
            bubble.SetColor(colorId, bubbleColors[Mathf.Clamp(colorId, 0, bubbleColors.Length - 1)]);
            currentBubble = bubble;
            canShoot = true;
            nextColorId = GetRandomColorId();
            GameManager.Instance?.OnNextBubbleChanged();
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

    public int GetNextColorId()
    {
        return nextColorId;
    }

    public Color GetColorById(int id)
    {
        if (bubbleColors == null || bubbleColors.Length == 0) return Color.white;
        return bubbleColors[Mathf.Clamp(id, 0, bubbleColors.Length - 1)];
    }
}