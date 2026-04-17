using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    public GameObject foodPrefab;

    public int minX = -8;
    public int maxX = 8;
    public int minY = -4;
    public int maxY = 4;

    private GameObject currentFood;
    void Start()
    {
        SpawnFood();
    }
    public void SpawnFood()
    {
        if (currentFood != null) return;

        Vector3 spawnPos = new Vector3(
            Random.Range(minX, maxX + 1),
            Random.Range(minY, maxY + 1),
            0
        );

        currentFood = Instantiate(foodPrefab, spawnPos, Quaternion.identity);
    }

    public void RemoveFood()
    {
        currentFood = null;
    }
}