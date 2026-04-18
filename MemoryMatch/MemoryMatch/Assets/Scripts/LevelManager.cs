using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData[] levels;
    public int currentLevel = 0;

    public LevelData GetCurrentLevel()
    {
        if (levels == null || levels.Length == 0)
        {
            Debug.LogError("LevelManager: no levels assigned in inspector.");
            return null;
        }

        if (currentLevel < 0)
            currentLevel = 0;

        if (currentLevel >= levels.Length)
            currentLevel = levels.Length - 1;

        return levels[currentLevel];
    }

    public bool NextLevel()
    {
        if (levels == null || levels.Length == 0)
            return false;

        if (currentLevel < levels.Length - 1)
        {
            currentLevel++;
            return true;
        }

        return false;
    }

    public void ResetLevels()
    {
        currentLevel = 0;
    }
}