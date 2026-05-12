using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class LeaderboardEntry
{
    public string gameName;
    public string playerName;
    public int score;
    public int level;
    public float timestamp;
    public string achievementsMeta;

    public LeaderboardEntry(string game, string player, int sc, int lv)
    {
        gameName = game;
        playerName = player;
        score = sc;
        level = lv;
        timestamp = Time.realtimeSinceStartup;
        achievementsMeta = "";
    }
}

public class GlobalLeaderboard : MonoBehaviour
{
    public static GlobalLeaderboard Instance { get; private set; }

    [SerializeField] private int maxEntriesPerGame = 10;
    private Dictionary<string, List<LeaderboardEntry>> leaderboards;

    const string LeaderboardSaveKey = "LEADERBOARD_";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadLeaderboards();
    }

    private void LoadLeaderboards()
    {
        leaderboards = new Dictionary<string, List<LeaderboardEntry>>();

        string[] games = { "BubbleShooter", "Tetris", "DinoRun", "Snake", "FlappyBird", "MemoryMatch" };

        foreach (string game in games)
        {
            leaderboards[game] = new List<LeaderboardEntry>();
            string key = LeaderboardSaveKey + game;

            if (PlayerPrefs.HasKey(key))
            {
                string json = PlayerPrefs.GetString(key);
                LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(json);
                if (data?.entries != null)
                {
                    leaderboards[game] = new List<LeaderboardEntry>(data.entries);
                }
            }
        }
    }

    public void AddScore(string gameName, string playerName, int score, int level, string achievements = "")
    {
        if (!leaderboards.ContainsKey(gameName))
            leaderboards[gameName] = new List<LeaderboardEntry>();

        LeaderboardEntry entry = new LeaderboardEntry(gameName, playerName, score, level);
        entry.achievementsMeta = achievements;

        leaderboards[gameName].Add(entry);

        // Sort by score descending
        leaderboards[gameName] = leaderboards[gameName]
            .OrderByDescending(e => e.score)
            .Take(maxEntriesPerGame)
            .ToList();

        SaveLeaderboard(gameName);
    }

    private void SaveLeaderboard(string gameName)
    {
        if (!leaderboards.ContainsKey(gameName))
            return;

        LeaderboardData data = new LeaderboardData();
        data.entries = leaderboards[gameName].ToArray();

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(LeaderboardSaveKey + gameName, json);
        PlayerPrefs.Save();
    }

    public List<LeaderboardEntry> GetLeaderboard(string gameName)
    {
        if (leaderboards.ContainsKey(gameName))
            return leaderboards[gameName];

        return new List<LeaderboardEntry>();
    }

    public LeaderboardEntry GetTopScore(string gameName)
    {
        if (leaderboards.ContainsKey(gameName) && leaderboards[gameName].Count > 0)
            return leaderboards[gameName][0];

        return null;
    }

    public void ResetLeaderboard(string gameName)
    {
        if (leaderboards.ContainsKey(gameName))
        {
            leaderboards[gameName].Clear();
            PlayerPrefs.DeleteKey(LeaderboardSaveKey + gameName);
            PlayerPrefs.Save();
        }
    }

    public void ResetAllLeaderboards()
    {
        foreach (var game in leaderboards.Keys.ToList())
        {
            ResetLeaderboard(game);
        }
    }

    [System.Serializable]
    private class LeaderboardData
    {
        public LeaderboardEntry[] entries;
    }
}
