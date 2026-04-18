using UnityEngine;

public class LogicScript : MonoBehaviour
{
    [ContextMenu("Add Score")]
    public void addScore(int scoreToAdd)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreToAdd);
        }
    }

    public void restartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    public void gameOver()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    public bool IsGameOver()
    {
        return GameManager.Instance != null && GameManager.Instance.IsGameOver();
    }
}
