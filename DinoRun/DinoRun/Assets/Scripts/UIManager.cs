using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject gameOverPanel;

    void Awake()
    {
        // Ensure there's an EventSystem in the scene, which is required for UI interaction
        if (EventSystem.current == null && FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystemInfo = new GameObject("EventSystem");
            eventSystemInfo.AddComponent<EventSystem>();
            eventSystemInfo.AddComponent<StandaloneInputModule>();
        }

        // Ensure the Canvas containing this UI actually has a GraphicRaycaster component
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.GetComponent<GraphicRaycaster>() == null)
        {
            canvas.gameObject.AddComponent<GraphicRaycaster>();
        }
    }

    public void StartGame()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
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