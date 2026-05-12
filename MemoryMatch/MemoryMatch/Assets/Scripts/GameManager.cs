using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int score = 0;
    public int level = 1;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;
    public TMP_Text levelText;
    public TMP_Text hintText;
    public float timeRemaining;
    public TMP_Text timerText;
    public int moveCount;
    public TMP_Text moveText;
    private float hintTimer = 0f;
    public AudioSource audioSource;
    public AudioClip flipSound;
    public AudioClip matchSound;
    public TMP_Text winText;
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject highScorePanel;
    public GameObject gameHUDPanel;
    public GameObject settingsPanel;
    public Button pauseButton;
    public Button restartButton;
    public Toggle soundToggle;
    public TMP_Dropdown difficultyDropdown;
    public BoardManager boardManager;
    public LevelManager levelManager;
    public bool isStarted = false;
    public int highScore = 0;

    [Header("Level Settings")]
    public int pairCount = 4;
    public float timeLimit = 60f;
    public int baseScore = 100;
    public float hintCooldown = 5f;
    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        soundToggle.isOn = PlayerPrefs.GetInt("Sound", 1) == 1;
        AudioListener.volume = soundToggle.isOn ? 1f : 0f;
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        highScorePanel.SetActive(false);
        gameHUDPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (boardManager != null)
            boardManager.boardParent.gameObject.SetActive(false);
        if (levelManager != null)
            levelManager.ResetLevels();

        Time.timeScale = 1f;
    }
    void Awake()
    {
        Instance = this;
    }
    private void Update()
    {
        if (!isStarted || isGameOver) return;

        // timer
        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            GameOver();
        }

        // hint cooldown
        if (levelManager != null && boardManager != null)
        {
            LevelData lvl = levelManager.GetCurrentLevel();
            if (lvl != null)
            {
                // nothing else here - hint timer in board
            }
        }

        if (timerText != null) timerText.text = Mathf.Ceil(timeRemaining).ToString();

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
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        if (levelManager != null)
            levelManager.ResetLevels();
        LevelData lvl = levelManager.GetCurrentLevel();
        level = 1;
        timeRemaining = lvl.timeLimit;
        hintTimer = 0f;
        moveCount = 0;
        score = 0;
        level = 1;

        isGameOver = false;
        if (boardManager != null)
        {
            boardManager.boardParent.gameObject.SetActive(true);
            boardManager.ResetBoard();
            boardManager.GenerateBoard();
        }
        UpdateUI();

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
    }
    public void BackToMenu()
    {
        mainMenuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        settingsPanel.SetActive(false);
        highScorePanel.SetActive(false);
        if (boardManager != null)
            boardManager.boardParent.gameObject.SetActive(false);
        isStarted = false;
        Time.timeScale = 0f;
    }
    public void SaveDifficulty()
    {
        PlayerPrefs.SetInt("Difficulty", difficultyDropdown.value);
    }
    public void SaveSound()
    {
        PlayerPrefs.SetInt("Sound", soundToggle.isOn ? 1 : 0);
        AudioListener.volume = soundToggle.isOn ? 1f : 0f;
    }
    public void PauseGame()
    {
        if (!isStarted || isGameOver) return;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }
    public void ResumeGame()
    {
        if (!isStarted || isGameOver) return;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }
    public void AddScore(int amount)
    {
        score += amount;
        NextLevel();
        UpdateUI();
    }
    public void UpdateUI()
    {
        scoreText.text = "Score: " + score;
        levelText.text = "Level: " + level;
        if (moveText != null) moveText.text = "Moves: " + moveCount;
        if (hintText != null) hintText.text = "Hint CD: " + Mathf.Ceil(hintTimer) + "s";
        if (timerText != null) timerText.text = "Time: " + Mathf.Ceil(timeRemaining) + "s";
    }
    public void NextLevel()
    {
        int newLevel = score / 50 + 1;

        if (newLevel > level)
        {
            level = newLevel;
            Debug.Log("Level Up! " + level);
        }
    }
    public bool isGameOver = false;

    public void GameOver()
    {
        isGameOver = true;
        isStarted = false;

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
    public void RestartGame()
    {
        gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
        pausePanel.SetActive(false);

        score = 0;
        level = 1;
        isGameOver = false;
        isStarted = false;
        if (boardManager != null) boardManager.ResetBoard();

        UpdateUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
    // UI-friendly restart entry point for Buttons (keeps behavior consistent with other UI methods)
    public void OnRestartPressed()
    {
        RestartGame();
    }
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public void WinGame()
    {
        Debug.Log("LEVEL COMPLETE");

        isStarted = false;
        Time.timeScale = 0f;

        if (winPanel != null)
            winPanel.SetActive(true);
        if (winText != null)
        {
            winText.text = "Score: " + score + "\nMoves: " + moveCount + "\nTime Left: " + Mathf.Ceil(timeRemaining);
        }
    }

    public void PlayFlip()
    {
        if (audioSource != null && flipSound != null)
            audioSource.PlayOneShot(flipSound);
    }

    public void PlayMatch()
    {
        if (audioSource != null && matchSound != null)
            audioSource.PlayOneShot(matchSound);
    }
    public void NextRound()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        bool hasNextLevel = false;

        if (levelManager != null)
            hasNextLevel = levelManager.NextLevel();

        if (!hasNextLevel)
        {
            Debug.Log("ALL LEVELS COMPLETED!");
            MainMenuAfterFinish();
            return;
        }

        LevelData lvl = levelManager.GetCurrentLevel();

        level = levelManager.currentLevel + 1;
        timeRemaining = lvl.timeLimit;
        moveCount = 0;

        if (boardManager != null)
        {
            boardManager.ResetBoard();
            boardManager.GenerateBoard();
        }

        UpdateUI();

        isStarted = true;
        Time.timeScale = 1f;
    }
    private void MainMenuAfterFinish()
    {
        if (boardManager != null)
            boardManager.boardParent.gameObject.SetActive(false);

        mainMenuPanel.SetActive(true);
        gameHUDPanel.SetActive(false);

        Time.timeScale = 1f;
    }
    // Forward selection API to BoardManager when available
    public void CardSelected(Card card)
    {
        if (boardManager != null)
            boardManager.CardSelected(card);
    }

    public bool CanSelect()
    {
        if (!isStarted || isGameOver)
            return false;

        return boardManager != null ? boardManager.CanSelect() : false;
    }
    public void UseHint()
    {
        if (!isStarted || isGameOver) return;

        if (boardManager != null)
            boardManager.TryUseHint();
    }
}