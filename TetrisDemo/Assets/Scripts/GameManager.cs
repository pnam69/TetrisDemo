using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject[] tetrominoes;
    public Transform spawnPoint;

    [Header("Visual Theme")]
    public Camera targetCamera;
    public Color cameraBackgroundColor = new Color(0.05f, 0.07f, 0.12f);

    public bool useThemeColors = true;
    public Color[] themeColors = new Color[]
    {
        new Color(0.24f, 0.82f, 0.96f),
        new Color(0.98f, 0.87f, 0.24f),
        new Color(0.66f, 0.51f, 0.95f),
        new Color(0.35f, 0.90f, 0.43f),
        new Color(0.97f, 0.34f, 0.34f),
        new Color(0.97f, 0.58f, 0.22f),
        new Color(0.30f, 0.51f, 0.98f)
    };

    public int score = 0;
    public Text scoreText;
    public Text linesText;
    public Text levelText;
    public Text nextPieceText;

    public float baseFallTime = 1f;
    public float minFallTime = 0.1f;
    public float speedUpPerLevel = 0.08f;

    public GameObject gameOverUI;
    public bool isGameOver = false;

    int totalLines = 0;
    int level = 1;
    readonly Queue<int> nextQueue = new Queue<int>();

    void Start()
    {
        ApplyVisualTheme();
        RefillBag();
        UpdateScoreUI();
        UpdateNextPieceUI();
        SpawnNew();
    }

    void ApplyVisualTheme()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam != null)
            cam.backgroundColor = cameraBackgroundColor;
    }

    public void SpawnNew()
    {
        if (isGameOver) return;
        if (tetrominoes == null || tetrominoes.Length == 0) return;

        int index = GetNextTetrominoIndex();
        GameObject piece = Instantiate(tetrominoes[index], spawnPoint.position, Quaternion.identity);
        ApplyThemeToPiece(piece, index);
        UpdateNextPieceUI();

        if (!Board.IsValidPosition(piece.transform))
        {
            Destroy(piece);
            GameOver();
        }
    }

    public void AddScore(int lines)
    {
        if (lines == 0) return;

        totalLines += lines;
        level = Mathf.Max(1, (totalLines / 10) + 1);

        switch (lines)
        {
            case 1: score += 100 * level; break;
            case 2: score += 300 * level; break;
            case 3: score += 500 * level; break;
            case 4: score += 800 * level; break;
        }

        UpdateScoreUI();
    }

    public void AddDropScore(int cells, bool hardDrop)
    {
        if (cells <= 0) return;

        score += hardDrop ? cells * 2 : cells;
        UpdateScoreUI();
    }

    public float GetFallTime()
    {
        float adjusted = baseFallTime - ((level - 1) * speedUpPerLevel);
        return Mathf.Max(minFallTime, adjusted);
    }

    int GetNextTetrominoIndex()
    {
        if (nextQueue.Count == 0)
            RefillBag();

        int index = nextQueue.Dequeue();

        if (nextQueue.Count == 0)
            RefillBag();

        return index;
    }

    void RefillBag()
    {
        List<int> bag = new List<int>();
        for (int i = 0; i < tetrominoes.Length; i++)
            bag.Add(i);

        for (int i = 0; i < bag.Count; i++)
        {
            int randomIndex = Random.Range(i, bag.Count);
            int temp = bag[i];
            bag[i] = bag[randomIndex];
            bag[randomIndex] = temp;
        }

        for (int i = 0; i < bag.Count; i++)
            nextQueue.Enqueue(bag[i]);
    }

    void UpdateNextPieceUI()
    {
        if (nextPieceText == null) return;

        if (tetrominoes == null || tetrominoes.Length == 0 || nextQueue.Count == 0)
        {
            nextPieceText.text = "Next: -";
            return;
        }

        nextPieceText.text = "Next: " + tetrominoes[nextQueue.Peek()].name;
    }

    void ApplyThemeToPiece(GameObject piece, int index)
    {
        if (!useThemeColors || piece == null) return;

        Color color = GetPieceColor(index);

        foreach (Transform block in piece.transform)
        {
            SpriteRenderer spriteRenderer = block.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                spriteRenderer.color = color;
        }
    }

    Color GetPieceColor(int index)
    {
        if (themeColors != null && themeColors.Length > 0)
            return themeColors[index % themeColors.Length];

        return Color.HSVToRGB((index * 0.14f) % 1f, 0.75f, 0.95f);
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;

        if (linesText != null)
            linesText.text = "Lines: " + totalLines;

        if (levelText != null)
            levelText.text = "Level: " + level;
    }
    void GameOver()
    {
        isGameOver = true;
        Debug.Log("GAME OVER");

        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }
    public bool IsGameOver()
    {
        return isGameOver;
    }
}