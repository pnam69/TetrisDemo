using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int scorePerPop = 10;
    public int scorePerDrop = 15;
    public float loseRow = -3.5f;
    public float loseWorldY = -3.5f;
    public int loseRowsAboveShooter = 1;
    public int level = 1;
    public int shotsPerLevel = 100;
    public float baseShootSpeed = 10f;
    public float speedIncreasePerLevel = 0.75f;
    public int score;
    public int combo;
    public int shotsLeft;
    public bool isGameOver;
    public bool isVictory;

    public TMP_Text scoreText;
    public TMP_Text levelText;
    public TMP_Text shotsText;
    public GameObject victoryPanel;
    public GameObject gameOverPanel;

    private GridSystem grid;
    private Shooter shooter;
    private bool pendingLoseCheck;
    private bool resolving;

    const string SaveLevelKey = "BS_Level";
    const string SaveBestScoreKey = "BS_BestScore";
    const string SaveShotsKey = "BS_ShotsLeft";
    const string SaveScoreKey = "BS_CurrentScore";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnsureRefs();
        AutoBindUIReferences();

        level = Mathf.Max(1, PlayerPrefs.GetInt(SaveLevelKey, 1));
        shotsLeft = shotsPerLevel;
        score = 0;
        combo = 0;
        isGameOver = false;
        isVictory = false;
        ApplyDifficulty();
        UpdateUI();
    }

    void Start()
    {
        AutoBindUIReferences();
        UpdateUI();
    }

    void ApplyDifficulty()
    {
        if (shooter != null)
        {
            shooter.shootSpeed = baseShootSpeed + (level - 1) * speedIncreasePerLevel;
        }
    }

    void UpdateUI()
    {
        // don't re-find refs every frame; only find if null
        EnsureRefs();
        AutoBindUIReferences();
        int bubblesLeft = grid != null ? grid.grid.Count : 0;

        if (scoreText != null) scoreText.text = "Score: " + score;
        if (levelText != null) levelText.text = "Level: " + level;
        if (shotsText != null) shotsText.text = "Shots Left: " + shotsLeft + "   Bubbles: " + bubblesLeft;
        if (victoryPanel != null) victoryPanel.SetActive(isVictory);
        if (gameOverPanel != null) gameOverPanel.SetActive(isGameOver);
    }

    public void OnBubbleShot()
    {
        if (isGameOver || isVictory) return;

        shotsLeft = Mathf.Max(0, shotsLeft - 1);

        SaveProgress();
        UpdateUI();
    }

    public void OnShotResolved(int poppedCount, int droppedCount)
    {
        if (isGameOver || isVictory) return;

        if (poppedCount >= 3)
        {
            combo++;
        }
        else
        {
            combo = 0;
        }

        int comboMultiplier = 1 + combo;
        score += poppedCount * scorePerPop * comboMultiplier;
        score += droppedCount * scorePerDrop;

        if (grid != null && grid.grid.Count == 0)
        {
            level++;
            isVictory = true;
            Debug.Log("Victory!");
            SaveProgress();
            UpdateUI();
            return;
        }

        // Delay lose check to next frame so physics and removals settle
        pendingLoseCheck = true;

        SaveProgress();
        UpdateUI();
    }

    void SaveProgress()
    {
        PlayerPrefs.SetInt(SaveLevelKey, level);
        PlayerPrefs.SetInt(SaveShotsKey, shotsLeft);
        PlayerPrefs.SetInt(SaveScoreKey, score);
        int best = PlayerPrefs.GetInt(SaveBestScoreKey, 0);
        if (score > best)
        {
            PlayerPrefs.SetInt(SaveBestScoreKey, score);
        }
        PlayerPrefs.Save();
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveProgress();
        }
    }

    void Update()
    {
        if (pendingLoseCheck)
        {
            pendingLoseCheck = false;
            CheckLoseCondition();
        }
        UpdateLoseLine();
    }

    void CheckLoseCondition()
    {
        if (isGameOver || isVictory) return;

        if (resolving) return;

        EnsureRefs();
        if (grid == null) return;

        if (IsGameOverByGrid())
        {
            TriggerGameOver();
            return;
        }
        if(shotsLeft <= 0)
        {
            TriggerGameOver();
            return;
        }
    }

    public void TriggerGameOver()
    {
        if (isGameOver || isVictory) return;

        isGameOver = true;
        Debug.Log("Game Over!");
        SaveProgress();
        UpdateUI();
    }

    bool IsGameOverByGrid()
    {
        EnsureRefs();
        if (grid == null) return false;

        int loseRow = GetLoseGridRowThreshold();

        foreach (var kv in grid.grid)
        {
            Vector2Int pos = kv.Key;

            if (pos.y <= loseRow)
                return true;
        }

        return false;
    }

    int GetLoseGridRowThreshold()
    {
        EnsureRefs();

        if (grid == null || shooter == null)
            return Mathf.FloorToInt(loseRow);

        Vector3 refPos = shooter.firePoint != null
            ? shooter.firePoint.position
            : shooter.transform.position;

        int shooterRow = grid.WorldToGrid(refPos).y;

        // Normal bubble-shooter behavior: lose when bubbles reach a row above shooter.
        return shooterRow + Mathf.Max(0, loseRowsAboveShooter);
    }

    public void SetResolving(bool value)
    {
        resolving = value;
    }

    void EnsureRefs()
    {
        if (grid == null) grid = FindObjectOfType<GridSystem>();
        if (shooter == null) shooter = FindObjectOfType<Shooter>();
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

    TMP_Text FindTMPByName(string objName)
    {
        GameObject go = FindObjectByName(objName);
        return go != null ? go.GetComponent<TMP_Text>() : null;
    }

    GameObject FindObjectByName(string objName)
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) return null;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int r = 0; r < roots.Length; r++)
        {
            Transform[] all = roots[r].GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == objName)
                    return all[i].gameObject;
            }
        }

        return null;
    }

    void OnApplicationQuit()
    {
        SaveProgress();
    }

    public void RestartCurrentLevel()
    {
        score = 0;
        combo = 0;
        shotsLeft = shotsPerLevel;
        isGameOver = false;
        isVictory = false;
        resolving = false;

        EnsureRefs();
        ApplyDifficulty();
        if (grid != null)
        {
            grid.LoadLayout();
        }

        pendingLoseCheck = false;
        UpdateUI();

        // force UI text to reflect the reset immediately
        if (scoreText != null) scoreText.text = "Score: 0";
        if (levelText != null) levelText.text = "Level: " + level;
        if (shotsText != null) shotsText.text = "Shots Left: " + shotsLeft + "   Bubbles: " + (grid != null ? grid.grid.Count : 0);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        
        // ensure shooter has a launcher bubble after restart
        EnsureRefs();
        if (shooter != null)
        {
            shooter.ResetShooter();
        }
        else
        {
            Debug.LogWarning("GameManager: Shooter not found on restart - cannot spawn launcher bubble.");
        }
    }

    public void StartButton()
    {
        RestartCurrentLevel();
    }

    public void RestartButton()
    {
        RestartCurrentLevel();
    }

    public void ExitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public LineRenderer loseLine;
    void UpdateLoseLine()
    {
        if (grid == null || loseLine == null) return;

        int loseRow = GetLoseGridRowThreshold();

        Vector2 left = grid.GridToWorld(new Vector2Int(-20, loseRow));
        Vector2 right = grid.GridToWorld(new Vector2Int(20, loseRow));

        loseLine.SetPosition(0, new Vector3(left.x, left.y, 0));
        loseLine.SetPosition(1, new Vector3(right.x, right.y, 0));
    }
}
