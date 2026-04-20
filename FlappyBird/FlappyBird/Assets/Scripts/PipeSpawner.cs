using UnityEngine;

public class PipeSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject pipePrefab;
    public float spawnInterval = 12.0f;
    public float initialDelay = 0f;
    public float minSpawnOffsetX = 4.8f;
    public float maxSpawnOffsetX = 6.0f;
    [Header("Gap Control")]
    public float minHorizontalGap = 4.5f;
    public bool logSpawns = false;
    
    [Header("Height Variation")]
    public float minHeight = -5.0f;
    public float maxHeight = 5.0f;
    
    private float timer = 0.0f;
    private LogicScript logic;
    private bool canSpawn;
    private float lastSpawnX = float.NegativeInfinity;

    void Start()
    {
        GameObject logicObject = GameObject.FindGameObjectWithTag("Logic");
        if (logicObject != null)
        {
            logic = logicObject.GetComponent<LogicScript>();
        }
        
        timer = -initialDelay;
        canSpawn = false;
    }

    public void BeginSpawning()
    {
        timer = -initialDelay;
        canSpawn = true;
    }

    public void StopSpawning()
    {
        canSpawn = false;
    }

    void Update()
    {
        if (pipePrefab == null)
        {
            Debug.LogWarning("Pipe Prefab is not assigned in PipeSpawner!");
            return;
        }
        
        if (!canSpawn)
        {
            return;
        }

        if (logic != null && logic.IsGameOver())
        {
            return;
        }
        
        timer += Time.deltaTime;

        float currentSpawnInterval = spawnInterval;
        if (GameManager.Instance != null)
        {
            currentSpawnInterval = GameManager.Instance.GetSpawnInterval();
        }
        
        if (timer >= currentSpawnInterval)
        {
            SpawnPipe();
            timer = 0.0f;
        }
    }

    void SpawnPipe()
    {
        float randomHeight = Random.Range(minHeight, maxHeight);
        float randomOffsetX = Random.Range(minSpawnOffsetX, maxSpawnOffsetX);
        float spawnX = transform.position.x + randomOffsetX;

        // enforce minimum horizontal gap from last spawned pipe
        if (!float.IsNegativeInfinity(lastSpawnX))
        {
            float gap = spawnX - lastSpawnX;
            if (gap < minHorizontalGap)
            {
                spawnX = lastSpawnX + minHorizontalGap;
            }
        }

        Vector3 spawnPosition = new Vector3(spawnX, randomHeight, 0);
        Instantiate(pipePrefab, spawnPosition, Quaternion.identity);

        lastSpawnX = spawnX;

        if (logSpawns)
        {
            float currentSpawnInterval = spawnInterval;
            if (GameManager.Instance != null)
                currentSpawnInterval = GameManager.Instance.GetSpawnInterval();

            Debug.Log($"Spawned pipe at x={spawnX:F2}, offset={randomOffsetX:F2}, interval={currentSpawnInterval:F2}");
        }
    }
}
