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
}