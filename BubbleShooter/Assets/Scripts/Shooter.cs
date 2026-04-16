using UnityEngine;
using UnityEngine.InputSystem;
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
    private Vector2 touchStartPos;
    private float touchStartTime;
    const float tapMaxDuration = 0.25f;
    const float tapMaxMove = 40f;
    private bool isAiming = false;
    private int activeFingerId = -1;
    [SerializeField] private float pullSpeedFactor = 5f;
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

    public void ResetShooter()
    {
        if (currentBubble != null)
        {
            Destroy(currentBubble.gameObject);
            currentBubble = null;
        }

        canShoot = true;

        if (bubblePrefab == null)
        {
            GridSystem grid = FindObjectOfType<GridSystem>();
            if (grid != null)
                bubblePrefab = grid.bubblePrefab;
        }

        if (bubbleColors == null || bubbleColors.Length == 0)
        {
            GridSystem grid = FindObjectOfType<GridSystem>();
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

        if (Input.touchCount > 0)
        {
            HandleTouchInput();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
            }
            else
            {
                isAiming = true;
                activeFingerId = -1;
                touchStartPos = Input.mousePosition;
                touchStartTime = Time.time;
                AimAtScreenPoint(touchStartPos);
            }
        }

        if (isAiming && Input.GetMouseButton(0))
        {
            AimAtScreenPoint(Input.mousePosition);
        }

        if (isAiming && Input.GetMouseButtonUp(0))
        {
            ReleaseAimAndShoot(Input.mousePosition);
            isAiming = false;
            activeFingerId = -1;
        }
    }

    bool WasShootPressedThisFrame()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Input.GetMouseButtonDown(0))
            return true;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
    }

    void HandleTouchInput()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
                continue;

            if (!isAiming && (t.phase == UnityEngine.TouchPhase.Began))
            {
                isAiming = true;
                activeFingerId = t.fingerId;
                touchStartPos = t.position;
                touchStartTime = Time.time;
                AimAtScreenPoint(t.position);
                return;
            }

            if (isAiming && t.fingerId == activeFingerId)
            {
                if (t.phase == UnityEngine.TouchPhase.Moved || t.phase == UnityEngine.TouchPhase.Stationary)
                {
                    AimAtScreenPoint(t.position);
                    return;
                }

                if (t.phase == UnityEngine.TouchPhase.Ended || t.phase == UnityEngine.TouchPhase.Canceled)
                {
                    ReleaseAimAndShoot(t.position);
                    isAiming = false;
                    activeFingerId = -1;
                    return;
                }
            }
        }
    }

    void ReleaseAimAndShoot(Vector2 releasePos)
    {
        if (!canShoot || firePoint == null || currentBubble == null) return;

        Vector2 pull = releasePos - touchStartPos;
        if (pull.magnitude < 5f)
        {
            Shoot();
            return;
        }

        Vector3 screenDir = (Vector3)(touchStartPos - releasePos);
        screenDir.z = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 worldTarget = Camera.main.ScreenToWorldPoint(screenDir + (Vector3)firePoint.position);

        Vector3 worldStart = Camera.main.ScreenToWorldPoint(new Vector3(touchStartPos.x, touchStartPos.y, Mathf.Abs(Camera.main.transform.position.z)));
        Vector3 worldRelease = Camera.main.ScreenToWorldPoint(new Vector3(releasePos.x, releasePos.y, Mathf.Abs(Camera.main.transform.position.z)));
        Vector2 worldPull = (Vector2)(worldRelease - worldStart);

        Vector2 shootDir = (-worldPull).normalized;
        float speed = Mathf.Clamp(worldPull.magnitude * pullSpeedFactor, 3f, 40f);

        currentBubble.Shoot(shootDir, speed);
        GameManager.Instance?.OnBubbleShot();
        currentBubble = null;
        canShoot = false;
        Invoke(nameof(SpawnBubble), spawnDelay);
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