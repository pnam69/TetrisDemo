using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI")]
    public GameObject mainMenuPanel;
    public GameObject gameHUDPanel;
    public GameObject gameOverPanel;
    public GameObject settingsPanel;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public TMP_Text gameOverScoreText;
    public TMP_Text comboText;
    public TMP_Text achievementText;
    public AchievementUI achievementUI;
    public Toggle soundToggle;
    public GameObject pausePanel;

    [Header("Gameplay")]
    public Bird bird;
    public PipeSpawner pipeSpawner;
    public int score;
    public int highScore;
    public bool isStarted;
    public bool isGameOver;

    [Header("Combo")]
    public float comboWindow = 2f;
    public int comboBonus = 1;

    [Header("Difficulty")]
    public float startPipeSpeed = 2.8f;
    public float maxPipeSpeed = 5.5f;
    public float speedRampPerSecond = 0.05f;
    public float startSpawnInterval = 1.45f;
    public float minSpawnInterval = 0.9f;
    public float spawnRampPerSecond = 0.01f;

    private float survivalTime;
    private float lastScoreTime = -999f;
    private int comboStreak;
    private bool reached1;
    private bool reached10;
    private bool reached50;

    private bool isPaused = false;
    public bool inputLocked = false;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        if (soundToggle != null)
        {
            bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
            soundToggle.isOn = isSoundOn;
            SaveSound(isSoundOn);
        }

        SetMenuState();
        UpdateUI();
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBgm();
        }
    }

    void Update()
    {
        if (!isStarted || isGameOver || isPaused)
        {
            return;
        }

        survivalTime += Time.deltaTime;
    }

    public void StartGame()
    {
        score = 0;
        comboStreak = 0;
        survivalTime = 0f;
        isGameOver = false;
        isStarted = true;
        reached10 = false;
        reached50 = false;
        isPaused = false;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);

        if (bird != null)
        {
            bird.BeginPlay();
        }

        if (pipeSpawner != null)
        {
            pipeSpawner.BeginSpawning();
        }

        UpdateUI();
        Time.timeScale = 1f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBgm();
        }
    }

    public void AddScore(int amount)
    {
        if (!isStarted || isGameOver)
        {
            return;
        }

        bool isCombo = Time.time - lastScoreTime <= comboWindow;
        comboStreak = isCombo ? comboStreak + 1 : 1;
        lastScoreTime = Time.time;

        int totalToAdd = amount + Mathf.Max(0, comboStreak - 1) * comboBonus;
        score += totalToAdd;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayScore();
        }

        CheckAchievements();
        UpdateUI();
    }

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        isStarted = false;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOver();
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
        UpdateUI();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBgm();
        }
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void BackToMenu()
    {
        SetMenuState();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBgm();
        }
    }

    public void SaveSound(bool isOn)
    {
        PlayerPrefs.SetInt("Sound", isOn ? 1 : 0);
        PlayerPrefs.Save();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSoundEnabled(isOn);
        }
    }

    public void SaveSound()
    {
        if (soundToggle == null)
        {
            return;
        }

        SaveSound(soundToggle.isOn);
    }

    public float GetPipeSpeed()
    {
        return Mathf.Min(maxPipeSpeed, startPipeSpeed + survivalTime * speedRampPerSecond);
    }

    public float GetSpawnInterval()
    {
        return Mathf.Max(minSpawnInterval, startSpawnInterval - survivalTime * spawnRampPerSecond);
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public bool IsStarted()
    {
        return isStarted;
    }

    private void SetMenuState()
    {
        isStarted = false;
        isGameOver = false;
        isPaused = false;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    private void CheckAchievements()
    {
        if (!reached1 && score >= 1)
        {
            reached1 = true;
            ShowAchievement("Achievement unlocked: First step");
        }
        if (!reached10 && score >= 10)
        {
            reached10 = true;
            ShowAchievement("Achievement unlocked: 10 points");
        }
        if (!reached50 && score >= 50)
        {
            reached50 = true;
            ShowAchievement("Achievement unlocked: 50 points");
        }
    }

    private void ShowAchievement(string message)
    {
        // Prefer a dedicated AchievementUI component if assigned
        if (achievementUI != null)
        {
            achievementUI.Show(message);
            return;
        }

        // Fallback to a single TMP text if present
        if (achievementText == null)
        {
            Debug.Log(message);
            return;
        }

        StopCoroutine(nameof(HideAchievementRoutine));
        achievementText.text = message;
        achievementText.gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(HideAchievementRoutine());
    }

    private IEnumerator HideAchievementRoutine()
    {
        yield return new WaitForSeconds(2f);

        if (achievementText != null)
        {
            achievementText.gameObject.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore;
        }

        if (gameOverScoreText != null)
        {
            gameOverScoreText.text = "Score: " + score + "\nHigh Score: " + highScore;
        }

        if (comboText != null)
        {
            comboText.text = comboStreak > 1 ? "Combo x" + comboStreak : string.Empty;
        }

        if (pausePanel != null) pausePanel.SetActive(isPaused);
    }

    public void PauseGame()
    {
        if (!isStarted || isGameOver) return;

        isPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null) pausePanel.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PauseBgm();
    }

    public void ResumeGame()
    {
        if (!isStarted || isGameOver) return;

        isPaused = false;
        Time.timeScale = 1f;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (AudioManager.Instance != null)
            AudioManager.Instance.ResumeBgm();

        StartCoroutine(UnlockInputAfterDelay());
    }
    private IEnumerator UnlockInputAfterDelay()
    {
        inputLocked = true;

        yield return new WaitForSeconds(0.2f);

        inputLocked = false;
    }
}
