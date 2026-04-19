using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float worldSpeed = 5f;
    public float speedIncrease = 0.1f;
    public float score = 0f;

    public bool gameStarted = false;
    public bool isGameOver = false;

    [Header("Day/Night")]
    public Camera mainCamera;
    public Color dayColor = new Color(0.53f, 0.81f, 0.92f, 1f);
    public Color nightColor = new Color(0.08f, 0.08f, 0.16f, 1f);
    public float dayNightCycleDuration = 20f;

    private float dayNightTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        ApplyDayNightColor();
    }

    void Update()
    {
        if (!gameStarted || isGameOver) return;

        worldSpeed += speedIncrease * Time.deltaTime;
        score += Time.deltaTime * 10f;
        dayNightTimer += Time.deltaTime;
        ApplyDayNightColor();
    }

    public void StartGame()
    {
        gameStarted = true;
        isGameOver = false;
    }

    public void GameOver()
    {
        isGameOver = true;
    }

    private void ApplyDayNightColor()
    {
        if (mainCamera == null || dayNightCycleDuration <= 0f)
        {
            return;
        }

        float t = Mathf.PingPong(dayNightTimer / dayNightCycleDuration, 1f);
        mainCamera.backgroundColor = Color.Lerp(dayColor, nightColor, t);
    }
}