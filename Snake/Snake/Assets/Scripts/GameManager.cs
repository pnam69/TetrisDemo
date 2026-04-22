using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private ObstacleManager obstacleManager;
    public int score = 0;
    public int level = 1;
    public float timer = 0f;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public TMP_Text levelText;
    public TMP_Text buffTimerText;
    public GameObject gameOverPanel;
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject highScorePanel;
    public GameObject gameHUDPanel;
    public GameObject settingsPanel;
    public Button pauseButton;
    public Toggle soundToggle;
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public TMP_Dropdown difficultyDropdown;
    public bool isStarted = false;
    public int highScore = 0;
    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        difficultyDropdown.value = PlayerPrefs.GetInt("Difficulty", 1);
        soundToggle.isOn = PlayerPrefs.GetInt("Sound", 1) == 1;
        // wire audio sources and restore mute state
        if (AudioManager.Instance != null)
        {
            if (bgmSource != null) AudioManager.Instance.bgmSource = bgmSource;
            if (sfxSource != null) AudioManager.Instance.sfxSource = sfxSource;
            // only forward audio sources and mute state; clips should be assigned on AudioManager
            AudioManager.Instance.SetMuted(!soundToggle.isOn);
            AudioManager.Instance.PlayBgm();
        }
        else
        {
            AudioListener.volume = soundToggle.isOn ? 1f : 0f;
        }
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        highScorePanel.SetActive(false);
        gameHUDPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);

        Time.timeScale = 0f;
    }
    void Awake()
    {
        Instance = this;
        obstacleManager = Object.FindFirstObjectByType<ObstacleManager>();
    }
    private void Update()
    {
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }
    public void StartGame()
    {
        isStarted = true;
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);

        score = 0;
        level = 1;
        timer = 0;
        isGameOver = false;

        UpdateUI();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBgm();

        Time.timeScale = 1f;
    }
    public void OpenHighScore()
    {
        mainMenuPanel.SetActive(false);
        highScorePanel.SetActive(true);

        highScoreText.text = "High Score: " + highScore;
    }
    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI();
    }
    public void BackToMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        highScorePanel.SetActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI();
    }
    public void SaveDifficulty()
    {
        PlayerPrefs.SetInt("Difficulty", difficultyDropdown.value);
    }
    public void SaveSound()
    {
        PlayerPrefs.SetInt("Sound", soundToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMuted(!soundToggle.isOn);
        }
        else
        {
            AudioListener.volume = soundToggle.isOn ? 1f : 0f;
        }
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI();
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        if (AudioManager.Instance != null) AudioManager.Instance.PlayUI();
    }
    public void AddScore(int amount)
    {
        score += amount;
        NextLevel();
        UpdateUI();
    }
    public void UpdateBuffTimer(float timeRemaining, Color color)
    {
        if (timeRemaining > 0f)
        {
            buffTimerText.color = color;
            buffTimerText.text = "Buff: " + timeRemaining.ToString("F1", CultureInfo.InvariantCulture) + "s";
        }
        else
        {
            buffTimerText.text = "";
        }
    }
    public void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        levelText.text = "Level: " + level;
        buffTimerText.text = "Buff Time: " + Mathf.FloorToInt(timer);
    }
    public void NextLevel()
    {
        int newLevel = score / 50 + 1;

        if (newLevel > level)
        {
            level = newLevel;
            Object.FindFirstObjectByType<SnakeController>().UpdateSpeed();
            if (obstacleManager != null) obstacleManager.SpawnObstacle();
            if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelUp();
            Debug.Log("Level Up! " + level);
        }
    }
    public bool isGameOver = false;

    public void GameOver()
    {
        isGameOver = true;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOverSfx();
            AudioManager.Instance.CrossfadeBGM(null, 0.6f);
        }
    }
    public void RestartGame()
    {
        gameOverPanel.SetActive(false);

        score = 0;
        level = 1;
        timer = 0.0f;
        isGameOver = false;

        UpdateUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}