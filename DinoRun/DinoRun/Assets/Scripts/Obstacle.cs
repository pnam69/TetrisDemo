using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public float spawnY = 1.0f;

    private ObstacleSpawner owner;
    private GameObject prefabKey;

    public void Initialize(ObstacleSpawner spawner, GameObject sourcePrefab)
    {
        owner = spawner;
        prefabKey = sourcePrefab;
    }

    public void Despawn()
    {
        if (owner != null && prefabKey != null)
        {
            owner.ReturnToPool(prefabKey, gameObject);
            return;
        }

        Destroy(gameObject);
    }
}

