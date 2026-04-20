using System.Collections;
using System.Collections.Generic;
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

    [Header("High Score Panel")]
    public GameObject highScorePanel;
    public TMP_Text highScorePanelScoreText;
    public TMP_Text highScorePanelAchievementsText;

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
    public float maxPipeSpeed = 10.5f;
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

        reached1 = PlayerPrefs.GetInt("Ach_Reached1", 0) == 1;
        reached10 = PlayerPrefs.GetInt("Ach_Reached10", 0) == 1;
        reached50 = PlayerPrefs.GetInt("Ach_Reached50", 0) == 1;

        if (soundToggle != null)
        {
            bool isSoundOn = PlayerPrefs.GetInt("Sound", 1) == 1;
            soundToggle.isOn = isSoundOn;
            SaveSound(isSoundOn);
        }

        SetMenuState();
        UpdateUI();
        UpdateHighScorePanel();
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
        if (bird != null) bird.gameObject.SetActive(true);
        score = 0;
        comboStreak = 0;
        survivalTime = 0f;
        isGameOver = false;
        isStarted = true;
        // do not clear persisted achievement flags here
        isPaused = false;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Ensure Bird.Start has executed before calling BeginPlay (so defaultGravityScale is set)
        if (bird != null)
        {
            StartCoroutine(BeginPlayNextFrame());
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
        UpdateHighScorePanel();
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
            UpdateHighScorePanel();
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGameOver();
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
        UpdateUI();
    }

    public void OpenHighScorePanel()
    {
        if (highScorePanel == null) return;
        UpdateHighScorePanel();
        highScorePanel.SetActive(true);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
    }

    public void CloseHighScorePanel()
    {
        if (highScorePanel != null) highScorePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    private void UpdateHighScorePanel()
    {
        if (highScorePanelScoreText != null)
        {
            highScorePanelScoreText.text = "High Score: " + highScore;
        }

        if (highScorePanelAchievementsText != null)
        {
            highScorePanelAchievementsText.text = GetAchievementsSummary();
        }
    }

    private string GetAchievementsSummary()
    {
        List<string> reached = new List<string>();
        if (reached1) reached.Add("First step");
        if (reached10) reached.Add("10 points");
        if (reached50) reached.Add("50 points");

        if (reached.Count == 0) return "No achievements yet.";

        return "Achievements:\n" + string.Join("\n", reached);
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
        isStarted = false;
        isGameOver = false;
        isPaused = false;
        inputLocked = false;
        Time.timeScale = 1f;

        if (pipeSpawner != null)
            pipeSpawner.StopSpawning();

        SetMenuState();
        if (highScorePanel != null) highScorePanel.SetActive(false);

        if (gameOverPanel != null)
        {
            var cg = gameOverPanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.blocksRaycasts = false;
                cg.interactable = false;
            }
        }

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
        if(highScorePanel != null) highScorePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (gameHUDPanel != null) gameHUDPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Clear gameplay state so menu is clean
        score = 0;
        comboStreak = 0;
        survivalTime = 0f;
        lastScoreTime = -999f;
        inputLocked = false;

        // Stop spawning and remove any existing pipes
        if (pipeSpawner != null)
            pipeSpawner.StopSpawning();

        var pipes = GameObject.FindObjectsOfType<PipeMove>();
        for (int i = 0; i < pipes.Length; i++)
        {
            if (pipes[i] != null)
                Destroy(pipes[i].gameObject);
        }

        // Deactivate bird while in menu (StartGame will reactivate)
        if (bird != null)
            bird.gameObject.SetActive(false);

        UpdateUI();
    }

    private void CheckAchievements()
    {
        if (!reached1 && score >= 1)
        {
            reached1 = true;
            ShowAchievement("Achievement unlocked: First step");
            PlayerPrefs.SetInt("Ach_Reached1", 1);
            PlayerPrefs.Save();
        }
        if (!reached10 && score >= 10)
        {
            reached10 = true;
            ShowAchievement("Achievement unlocked: 10 points");
            PlayerPrefs.SetInt("Ach_Reached10", 1);
            PlayerPrefs.Save();
        }
        if (!reached50 && score >= 50)
        {
            reached50 = true;
            ShowAchievement("Achievement unlocked: 50 points");
            PlayerPrefs.SetInt("Ach_Reached50", 1);
            PlayerPrefs.Save();
        }
    }

    private void ShowAchievement(string message)
    {
        if (achievementUI != null)
        {
            achievementUI.Show(message);
            return;
        }

        if (achievementText == null)
        {
            Debug.Log(message);
            return;
        }

        StopAllCoroutines();
        achievementText.text = message;
        achievementText.gameObject.SetActive(true);
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

    private IEnumerator BeginPlayNextFrame()
    {
        yield return null; // wait one frame
        if (bird != null)
            bird.BeginPlay();
    }

    public void SetHighScore(int value)
    {
        highScore = value;
        PlayerPrefs.SetInt("HighScore", value);

        PlayerPrefs.DeleteKey("Ach_Reached1");
        PlayerPrefs.DeleteKey("Ach_Reached10");
        PlayerPrefs.DeleteKey("Ach_Reached50");
        PlayerPrefs.Save();

        reached1 = reached10 = reached50 = false;

        UpdateHighScorePanel();
        UpdateUI();
    }
}

