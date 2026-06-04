using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public CameraShake cameraShake;
    public float worldSpeed = 5f;
    public float speedIncrease = 0.1f;
    public float score = 0f;

    public bool gameStarted = false;
    public bool isGameOver = false;

    [Header("Day/Night")]
    public Camera mainCamera;
    public Color dayColor = new Color(0.53f, 0.81f, 0.92f, 1f);
    public Color nightColor = new Color(0.08f, 0.08f, 0.16f, 1f);
    public float dayNightCycleDuration = 15f;

    private float dayNightTimer;

    [Header("Audio")]
    public AudioSource audioSource;

    public AudioClip jumpSfx;
    public AudioClip slideSfx;
    public AudioClip deathSfx;
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
        score = 0;
    }

    public void GameOver()
    {
        isGameOver = true;

        if (cameraShake != null)
        {
            cameraShake.Shake(0.3f, 0.15f);
        }
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
    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(jumpSfx);
    }

    public void PlaySlideSound()
    {
        audioSource.PlayOneShot(slideSfx);
    }

    public void PlayDeathSound()
    {
        audioSource.PlayOneShot(deathSfx);
    }
}