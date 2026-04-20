using System.Collections.Generic;
using UnityEngine;

public class SnakeController : MonoBehaviour
{
    [Header("Movement")]
    public float baseMoveInterval = 0.2f;
    public float speedIncreasePerLevel = 0.015f;
    private float temporarySpeedModifier = 0f;
    public float moveInterval;
    private float moveTimer;
    private Vector2Int direction = Vector2Int.right;
    private bool directionChangedThisStep = false;

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

        int difficulty = PlayerPrefs.GetInt("Difficulty", 1);

        switch (difficulty)
        {
            case 0:
                baseMoveInterval = 0.35f;
                break;

            case 1:
                baseMoveInterval = 0.28f;
                break;

            case 2:
                baseMoveInterval = 0.24f;
                break;
        }

        // Initialize moveInterval based on current settings
        UpdateSpeed();

        foodSpawner = Object.FindFirstObjectByType<FoodSpawner>();
    }

    void Update()
    {
        if (!GameManager.Instance.isStarted || GameManager.Instance.isGameOver) return;

        HandleInput();
        moveTimer += Time.deltaTime;
        if (moveTimer >= moveInterval)
        {
            moveTimer = 0f;
            Move();
        }
    }

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
            SetDirection(Vector2Int.up);

        if (Input.GetKeyDown(KeyCode.DownArrow))
            SetDirection(Vector2Int.down);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            SetDirection(Vector2Int.left);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            SetDirection(Vector2Int.right);
    }

    public void SetDirection(Vector2Int newDirection)
    {
        // Prevent processing multiple direction changes between moves
        if (directionChangedThisStep) return;

        // Prevent direct 180-degree opposite direction
        if (newDirection == Vector2Int.up && direction == Vector2Int.down) return;
        if (newDirection == Vector2Int.down && direction == Vector2Int.up) return;
        if (newDirection == Vector2Int.left && direction == Vector2Int.right) return;
        if (newDirection == Vector2Int.right && direction == Vector2Int.left) return;

        // Prevent turning into the first body segment (instant self-bite)
        if (bodySegments.Count > 0)
        {
            Vector3 nextPos = transform.position + new Vector3(newDirection.x, newDirection.y, 0);
            if (bodySegments[0].position == nextPos)
                return;
        }

        direction = newDirection;
        directionChangedThisStep = true;
    }
    public void UpdateSpeed()
    {
        moveInterval = baseMoveInterval - (GameManager.Instance.level - 1) * speedIncreasePerLevel + temporarySpeedModifier;

        if (moveInterval < 0.05f)
            moveInterval = 0.05f;
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

        // allow direction changes again after the snake has moved
        directionChangedThisStep = false;
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
            Food food = other.GetComponent<Food>();
            FoodType eatenType = food != null ? food.foodType : FoodType.Normal;

            Grow();

            switch (eatenType)
            {
                case FoodType.Normal:
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayFood(FoodType.Normal);
                    GameManager.Instance.AddScore(10);
                    temporarySpeedModifier = 0f;
                    break;

                case FoodType.SpeedBoost:
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayFood(FoodType.SpeedBoost);
                    GameManager.Instance.AddScore(15);
                    temporarySpeedModifier = -0.03f;
                    break;

                case FoodType.Slow:
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayFood(FoodType.Slow);
                    GameManager.Instance.AddScore(5);
                    temporarySpeedModifier = 0.03f;
                    break;
            }

            UpdateSpeed();

            Destroy(other.gameObject);
            foodSpawner.RemoveFood();
            foodSpawner.SpawnFood();
        }
        if (other.CompareTag("Wall") || other.CompareTag("Body") || other.CompareTag("Obstacle"))
        {
            GameManager.Instance.GameOver();
        }
    }
}