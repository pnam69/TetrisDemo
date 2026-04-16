using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameOverPanel;

    public void StartGame()
    {
        startPanel.SetActive(false);
        GameManager.Instance.StartGame();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #else
            Application.Quit();
    #endif
    }

    void Update()
    {
        if (GameManager.Instance.isGameOver)
        {
            gameOverPanel.SetActive(true);
        }
    }
}