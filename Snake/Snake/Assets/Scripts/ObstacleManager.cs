using System.Collections.Generic;
using UnityEngine;

public class ObstacleManager : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public Transform snakeHead;
    public int minX = -13;
    public int maxX = 13;
    public int minY = -7;
    public int maxY = 7;

    private List<GameObject> obstacles = new List<GameObject>();

    public void SpawnObstacle()
    {
        Vector3 spawnPos;

        do
        {
            spawnPos = new Vector3(
                Random.Range(minX, maxX + 1),
                Random.Range(minY, maxY + 1),
                0
            );
        }
        while (Physics2D.OverlapCircle(spawnPos, 0.1f) != null);

        GameObject obstacle = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
        obstacles.Add(obstacle);
    }
}