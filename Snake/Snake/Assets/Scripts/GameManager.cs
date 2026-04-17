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
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public GameObject gameOverPanel;
    public GameObject startPanel;
    public GameObject pausePanel;
    public Button pauseButton;
    public bool isStarted = false;
    void Start()
    {
        UpdateUI();
        gameOverPanel.SetActive(false);
        startPanel.SetActive(true);
        pausePanel.SetActive(false);
        Time.timeScale = 0f; // Pause the game at the start
    }
    void Awake()
    {
        Instance = this;
        obstacleManager = FindObjectOfType<ObstacleManager>();
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
        startPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        Time.timeScale = 1f;
    }
    public void PauseGame()
    {
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
    }
    public void ResumeGame()
    {
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
    }
    public void NextLevel()
    {
        int newLevel = score / 50 + 1;

        if (newLevel > level)
        {
            level = newLevel;
            FindObjectOfType<SnakeController>().UpdateSpeed();
            obstacleManager.SpawnObstacle();
            Debug.Log("Level Up! " + level);
        }
    }
    public bool isGameOver = false;

    public void GameOver()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        startPanel.SetActive(false);
        pausePanel.SetActive(false);

        Time.timeScale = 0f;
    }
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

}