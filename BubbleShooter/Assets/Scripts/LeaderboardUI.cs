using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform leaderboardContainer;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private TextMeshProUGUI leaderboardTitle;
    [SerializeField] private Button nextGameButton;
    [SerializeField] private Button prevGameButton;

    private string[] games = { "BubbleShooter", "Tetris", "DinoRun", "Snake", "FlappyBird", "MemoryMatch" };
    private int currentGameIndex = 0;

    void Start()
    {
        if (nextGameButton != null)
            nextGameButton.onClick.AddListener(ShowNextGame);
        if (prevGameButton != null)
            prevGameButton.onClick.AddListener(ShowPrevGame);

        DisplayLeaderboard();
    }

    public void DisplayLeaderboard(string gameName = null)
    {
        if (gameName != null)
        {
            for (int i = 0; i < games.Length; i++)
            {
                if (games[i] == gameName)
                {
                    currentGameIndex = i;
                    break;
                }
            }
        }

        string currentGame = games[currentGameIndex];

        if (leaderboardTitle != null)
            leaderboardTitle.text = $"{currentGame} - Top Scores";

        // Clear existing entries
        if (leaderboardContainer != null)
        {
            foreach (Transform child in leaderboardContainer)
            {
                Destroy(child.gameObject);
            }
        }

        // Get and display leaderboard
        if (GlobalLeaderboard.Instance != null)
        {
            List<LeaderboardEntry> entries = GlobalLeaderboard.Instance.GetLeaderboard(currentGame);

            if (entries.Count == 0)
            {
                CreateEmptyEntry("No scores yet");
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                CreateLeaderboardEntry(entries[i], i + 1);
            }
        }
    }

    private void CreateLeaderboardEntry(LeaderboardEntry entry, int rank)
    {
        if (leaderboardEntryPrefab == null || leaderboardContainer == null)
            return;

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        TextMeshProUGUI rankText = entryObj.transform.Find("Rank")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI nameText = entryObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI scoreText = entryObj.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();

        if (rankText != null)
            rankText.text = rank.ToString();
        if (nameText != null)
            nameText.text = entry.playerName;
        if (scoreText != null)
            scoreText.text = entry.score.ToString();

        // Add medal for top 3
        Image medalImage = entryObj.GetComponent<Image>();
        if (medalImage != null)
        {
            switch (rank)
            {
                case 1:
                    medalImage.color = new Color(1f, 0.84f, 0f); // Gold
                    break;
                case 2:
                    medalImage.color = new Color(0.75f, 0.75f, 0.75f); // Silver
                    break;
                case 3:
                    medalImage.color = new Color(0.8f, 0.5f, 0.2f); // Bronze
                    break;
                default:
                    medalImage.color = Color.white;
                    break;
            }
        }
    }

    private void CreateEmptyEntry(string message)
    {
        if (leaderboardEntryPrefab == null || leaderboardContainer == null)
            return;

        GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);
        TextMeshProUGUI text = entryObj.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = message;
    }

    private void ShowNextGame()
    {
        currentGameIndex = (currentGameIndex + 1) % games.Length;
        DisplayLeaderboard();
    }

    private void ShowPrevGame()
    {
        currentGameIndex = (currentGameIndex - 1 + games.Length) % games.Length;
        DisplayLeaderboard();
    }
}
