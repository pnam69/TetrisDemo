using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public Transform spawnPoint;

    public float minSpawnTime = 2f;
    public float maxSpawnTime = 2.5f;

    private float timer;
    private float nextSpawnTime;

    void Start()
    {
        SetNextSpawnTime();
    }

    void Update()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.gameStarted) return;
        if (GameManager.Instance.isGameOver) return;

        timer += Time.deltaTime;

        if (timer >= nextSpawnTime)
        {
            SpawnObstacle();
            timer = 0f;
            SetNextSpawnTime();
        }
    }

    void SpawnObstacle()
    {
        int index = Random.Range(0, obstaclePrefabs.Length);

        GameObject prefab = obstaclePrefabs[index];
        Obstacle obstacleData = prefab.GetComponent<Obstacle>();

        Vector3 spawnPos = new Vector3(
            spawnPoint.position.x,
            obstacleData.spawnY,
            0f
        );

        Instantiate(prefab, spawnPos, Quaternion.identity);
    }

    void SetNextSpawnTime()
    {
        nextSpawnTime = Random.Range(minSpawnTime, maxSpawnTime);
    }
}