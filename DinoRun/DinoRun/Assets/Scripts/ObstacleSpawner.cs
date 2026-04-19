using UnityEngine;
using System.Collections.Generic;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject[] obstaclePrefabs;
    public Transform spawnPoint;

    public float minSpawnTime = 2f;
    public float maxSpawnTime = 2.5f;
    public float minSpawnTimeAtMaxSpeed = 0.9f;
    public float maxSpawnTimeAtMaxSpeed = 1.4f;
    public float maxDifficultySpeed = 14f;
    public int poolSizePerObstacle = 5;

    private float timer;
    private float nextSpawnTime;
    private float baseWorldSpeed;
    private int bagCursor;

    private readonly List<int> spawnBag = new List<int>();
    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();

    void Start()
    {
        if (GameManager.Instance != null)
        {
            baseWorldSpeed = GameManager.Instance.worldSpeed;
        }

        BuildSpawnBag();
        WarmPool();
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
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0 || spawnPoint == null)
        {
            return;
        }

        int index = NextObstacleIndex();

        GameObject prefab = obstaclePrefabs[index];
        Obstacle obstacleData = prefab.GetComponent<Obstacle>();
        float spawnY = obstacleData != null ? obstacleData.spawnY : spawnPoint.position.y;

        Vector3 spawnPos = new Vector3(
            spawnPoint.position.x,
            spawnY,
            0f
        );

        GameObject obstacleInstance = GetFromPool(prefab);
        obstacleInstance.transform.position = spawnPos;
        obstacleInstance.transform.rotation = Quaternion.identity;

        Obstacle runtimeObstacle = obstacleInstance.GetComponent<Obstacle>();
        if (runtimeObstacle != null)
        {
            runtimeObstacle.Initialize(this, prefab);
        }

        obstacleInstance.SetActive(true);
    }

    void SetNextSpawnTime()
    {
        float speed = GameManager.Instance != null ? GameManager.Instance.worldSpeed : baseWorldSpeed;
        float t = Mathf.InverseLerp(baseWorldSpeed, maxDifficultySpeed, speed);
        float currentMin = Mathf.Lerp(minSpawnTime, minSpawnTimeAtMaxSpeed, t);
        float currentMax = Mathf.Lerp(maxSpawnTime, maxSpawnTimeAtMaxSpeed, t);
        nextSpawnTime = Random.Range(currentMin, currentMax);
    }

    int NextObstacleIndex()
    {
        if (spawnBag.Count == 0)
        {
            BuildSpawnBag();
        }

        if (bagCursor >= spawnBag.Count)
        {
            bagCursor = 0;
            ShuffleBag();
        }

        int index = spawnBag[bagCursor];
        bagCursor++;
        return index;
    }

    void BuildSpawnBag()
    {
        spawnBag.Clear();
        bagCursor = 0;

        if (obstaclePrefabs == null)
        {
            return;
        }

        for (int i = 0; i < obstaclePrefabs.Length; i++)
        {
            spawnBag.Add(i);
        }

        ShuffleBag();
    }

    void ShuffleBag()
    {
        for (int i = spawnBag.Count - 1; i > 0; i--)
        {
            int swap = Random.Range(0, i + 1);
            int temp = spawnBag[i];
            spawnBag[i] = spawnBag[swap];
            spawnBag[swap] = temp;
        }
    }

    void WarmPool()
    {
        if (obstaclePrefabs == null)
        {
            return;
        }

        for (int i = 0; i < obstaclePrefabs.Length; i++)
        {
            GameObject prefab = obstaclePrefabs[i];
            if (prefab == null)
            {
                continue;
            }

            if (!pools.ContainsKey(prefab))
            {
                pools[prefab] = new Queue<GameObject>();
            }

            for (int j = 0; j < poolSizePerObstacle; j++)
            {
                GameObject instance = Instantiate(prefab, transform);
                instance.SetActive(false);
                pools[prefab].Enqueue(instance);
            }
        }
    }

    GameObject GetFromPool(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
        }

        Queue<GameObject> queue = pools[prefab];
        while (queue.Count > 0)
        {
            GameObject instance = queue.Dequeue();
            if (instance != null)
            {
                return instance;
            }
        }

        GameObject created = Instantiate(prefab, transform);
        created.SetActive(false);
        return created;
    }

    public void ReturnToPool(GameObject prefab, GameObject instance)
    {
        if (prefab == null || instance == null)
        {
            return;
        }

        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
        }

        instance.SetActive(false);
        instance.transform.SetParent(transform);
        pools[prefab].Enqueue(instance);
    }
}