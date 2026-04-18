using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "MemoryMatch/Level")]
public class LevelData : ScriptableObject
{
    public int pairCount;
    public float timeLimit;
    public int baseScore;
    public float hintCooldown;
}