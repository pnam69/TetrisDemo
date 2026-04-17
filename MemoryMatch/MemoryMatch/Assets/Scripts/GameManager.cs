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
    public GameObject gameOverPanel;
    public GameObject winPanel;
    public GameObject mainMenuPanel;
    public GameObject pausePanel;
    public GameObject highScorePanel;
    public GameObject gameHUDPanel;
    public GameObject settingsPanel;
    public Button pauseButton;
    public Toggle soundToggle;
    public TMP_Dropdown difficultyDropdown;
    public GameObject cardPrefab;
    public Transform boardParent;
    private Card firstCard;
    private Card secondCard;
    private int matchedPairs = 0;
    private bool canSelect = true;
    public bool isStarted = false;
    public int highScore = 0;

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        difficultyDropdown.value = PlayerPrefs.GetInt("Difficulty", 1);
        soundToggle.isOn = PlayerPrefs.GetInt("Sound", 1) == 1;
        AudioListener.volume = soundToggle.isOn ? 1f : 0f;
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        highScorePanel.SetActive(false);
        gameHUDPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        GenerateBoard();
        Time.timeScale = 0f;
    }
    void Awake()
    {
        Instance = this;
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
        mainMenuPanel.SetActive(false);
        gameHUDPanel.SetActive(true);

        score = 0;
        level = 1;
        isGameOver = false;
        matchedPairs = 0;

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
        settingsPanel.SetActive(false);
        highScorePanel.SetActive(false);
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
            Debug.Log("Level Up! " + level);
        }
    }
    public bool isGameOver = false;

    public void GameOver()
    {
        isGameOver = true;

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

        score = 0;
        level = 1;
        isGameOver = false;
        matchedPairs = 0;

        UpdateUI();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    public void WinGame()
    {
        Debug.Log("YOU WIN");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    void GenerateBoard()
    {
        List<int> cardIDs = new List<int>() { 0, 0, 1, 1 };

        for (int i = 0; i < cardIDs.Count; i++)
        {
            int randomIndex = Random.Range(i, cardIDs.Count);
            int temp = cardIDs[i];
            cardIDs[i] = cardIDs[randomIndex];
            cardIDs[randomIndex] = temp;
        }

        foreach (int id in cardIDs)
        {
            GameObject cardObj = Instantiate(cardPrefab, boardParent);
            Card card = cardObj.GetComponent<Card>();
            card.cardID = id;
        }
    }
    public void CardSelected(Card card)
    {
        if (!canSelect) return;
        if (firstCard == null)
        {
            firstCard = card;
        }
        else if (secondCard == null)
        {
            secondCard = card;
            StartCoroutine(CheckMatch());
        }
    }
    private IEnumerator CheckMatch()
    {
        canSelect = false;

        yield return new WaitForSeconds(0.5f);

        if (firstCard.cardID == secondCard.cardID)
        {
            firstCard.Match();
            secondCard.Match();

            matchedPairs++;

            if (matchedPairs >= boardParent.childCount / 2)
            {
                WinGame();
            }
        }
        else
        {
            firstCard.Hide();
            secondCard.Hide();
        }

        firstCard = null;
        secondCard = null;

        canSelect = true;
    }
    public bool CanSelect()
    {
        return canSelect;
    }
}