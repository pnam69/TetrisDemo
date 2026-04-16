using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public float spawnRate = 0.0f;
    public float spawnX = 15f;
    public float minY = 0f;
    public float maxY = 9f;

    private float timer;
    private void Start()
    {
        spawnRate = Random.Range(2.2f, 3.2f);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if(GameManager.Instance == null) return;
        if(GameManager.Instance.isGameOver) return;
        if(GameManager.Instance.gameStarted == false) return;
        if (timer >= spawnRate)
        {
            Spawn();
            timer = 0f;
        }
    }

    void Spawn()
    {
        Vector3 pos = new Vector3(
            spawnX,

            Random.Range(minY, maxY),
            0
        );

        Instantiate(obstaclePrefab, pos, Quaternion.identity);
    }
}