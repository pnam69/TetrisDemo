using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float worldSpeed = 5f;
    public float speedIncrease = 0.1f;

    public float score = 0f;

    public bool gameStarted = false;
    public bool isGameOver = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!gameStarted || isGameOver) return;

        worldSpeed += speedIncrease * Time.deltaTime;
        score += Time.deltaTime * 10f;
    }

    public void StartGame()
    {
        gameStarted = true;
    }

    public void GameOver()
    {
        isGameOver = true;
    }

    public void SetResolving(bool value)
    {
        resolving = value;
    }

    void EnsureRefs()
    {
        // Use the newer Object API to avoid obsolete FindObjectOfType warnings in newer Unity versions
        if (grid == null) grid = Object.FindFirstObjectByType<GridSystem>();
        if (shooter == null) shooter = Object.FindFirstObjectByType<Shooter>();
    }

    void AutoBindUIReferences()
    {
        if (scoreText == null) scoreText = FindTMPByName("ScoreText");
        if (levelText == null) levelText = FindTMPByName("LevelText");
        if (shotsText == null) shotsText = FindTMPByName("ShotText") ?? FindTMPByName("ShotsText");

        if (victoryPanel == null) victoryPanel = FindObjectByName("WinPanel") ?? FindObjectByName("VictoryPanel");
        if (gameOverPanel == null) gameOverPanel = FindObjectByName("GameOverPanel");
        if (loseLine == null)
        {
            GameObject lineGo = FindObjectByName("LoseLine") ?? FindObjectByName("Lose Line");
            if (lineGo != null) loseLine = lineGo.GetComponent<LineRenderer>();
        }
    }
}