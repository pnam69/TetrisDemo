using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject normalFoodPrefab;
    public GameObject speedFoodPrefab;
    public GameObject slowFoodPrefab;
    public Transform snakeHead;
    public int minX = -13;
    public int maxX = 13;
    public int minY = -7;
    public int maxY = 7;

    private GameObject currentFood;
    void Start()
    {
        SpawnFood();
    }
    public void SpawnFood()
    {
        if (currentFood != null) return;

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

        int randomType = Random.Range(0, 3);

        GameObject prefabToSpawn = normalFoodPrefab;

        if (randomType == 1)
            prefabToSpawn = speedFoodPrefab;
        else if (randomType == 2)
            prefabToSpawn = slowFoodPrefab;

        currentFood = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
    }

    public void RemoveFood()
    {
        currentFood = null;
    }
}