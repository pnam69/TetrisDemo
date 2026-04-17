using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Movement")]
    public float moveInterval = 0.2f;
    private float moveTimer;
    private Vector2Int direction = Vector2Int.right;

    [Header("Body")]
    public GameObject bodyPrefab;
    public int startingBodySize = 2;

    private List<Transform> bodySegments = new List<Transform>();
    private List<Vector3> previousPositions = new List<Vector3>();
    private FoodSpawner foodSpawner;
    void Start()
    {
        for (int i = 0; i < startingBodySize; i++)
        {
            Grow();
        }
        foodSpawner = FindObjectOfType<FoodSpawner>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            SetDirection(Vector2Int.up);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            SetDirection(Vector2Int.down);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SetDirection(Vector2Int.left);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SetDirection(Vector2Int.right);

        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Move();
        }
    }

    public void SetDirection(Vector2Int newDirection)
    {
        if (newDirection == Vector2Int.up && direction == Vector2Int.down) return;
        if (newDirection == Vector2Int.down && direction == Vector2Int.up) return;
        if (newDirection == Vector2Int.left && direction == Vector2Int.right) return;
        if (newDirection == Vector2Int.right && direction == Vector2Int.left) return;

        direction = newDirection;
    }

    void Move()
    {
        previousPositions.Clear();

        previousPositions.Add(transform.position);

        foreach (Transform segment in bodySegments)
        {
            previousPositions.Add(segment.position);
        }

        transform.position += new Vector3(direction.x, direction.y, 0);

        for (int i = 0; i < bodySegments.Count; i++)
        {
            bodySegments[i].position = previousPositions[i];
        }
    }

    public void Grow()
    {
        Vector3 spawnPosition;

        if (bodySegments.Count == 0)
            spawnPosition = transform.position - new Vector3(direction.x, direction.y, 0);
        else
            spawnPosition = bodySegments[bodySegments.Count - 1].position;

        GameObject newSegment = Instantiate(bodyPrefab, spawnPosition, Quaternion.identity);
        bodySegments.Add(newSegment.transform);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Food"))
        {
            Grow();
            GameManager.Instance.AddScore(10);
            Destroy(other.gameObject);
            foodSpawner.RemoveFood();
            foodSpawner.SpawnFood();
        }
    }
}