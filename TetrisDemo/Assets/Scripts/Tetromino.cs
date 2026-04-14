using UnityEngine;

public class Tetromino : MonoBehaviour
{
    float previousTime;
    GameManager gm;
    Transform ghostRoot;

    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        CreateGhost();
        UpdateGhost();
    }
    void Update()
    {
        if (gm == null) return;

        // stop if game over
        if (gm.IsGameOver()) return;

        HandleInput();
        HandleFall();
        UpdateGhost();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
            Move(Vector3.left);

        if (Input.GetKeyDown(KeyCode.RightArrow))
            Move(Vector3.right);

        if (Input.GetKeyDown(KeyCode.UpArrow))
            Rotate();

        if (Input.GetKeyDown(KeyCode.Space))
            HardDrop();
    }

    void HandleFall()
    {
        bool isSoftDrop = Input.GetKey(KeyCode.DownArrow);
        float fallTime = isSoftDrop ? 0.05f : gm.GetFallTime();

        if (Time.time - previousTime > fallTime)
        {
            transform.position += Vector3.down;

            if (Board.IsValidPosition(transform))
            {
                //if (isSoftDrop)
                //    gm.AddDropScore(1, false);
            }
            else
            {
                transform.position += Vector3.up;
                LockPiece();
            }

            previousTime = Time.time;
        }
    }

    void HardDrop()
    {
        int droppedCells = 0;

        while (true)
        {
            transform.position += Vector3.down;

            if (!Board.IsValidPosition(transform))
            {
                transform.position += Vector3.up;
                break;
            }

            droppedCells++;
        }

        gm.AddDropScore(droppedCells, true);
        LockPiece();
    }

    void Move(Vector3 dir)
    {
        transform.position += dir;

        if (!Board.IsValidPosition(transform))
            transform.position -= dir;
    }

    void Rotate()
    {
        transform.Rotate(0, 0, 90);

        if (!Board.IsValidPosition(transform))
        {
            if (TryMove(Vector3.right)) return;
            if (TryMove(Vector3.left)) return;
            if (TryMove(Vector3.up)) return;

            transform.Rotate(0, 0, -90);
        }
    }

    bool TryMove(Vector3 dir)
    {
        transform.position += dir;

        if (Board.IsValidPosition(transform))
            return true;

        transform.position -= dir;
        return false;
    }

    void LockPiece()
    {
        if (ghostRoot != null)
            Destroy(ghostRoot.gameObject);

        AddToGrid();

        int lines = Board.ClearLines();
        gm.AddScore(lines);

        gm.SpawnNew();
        enabled = false;
    }

    void CreateGhost()
    {
        ghostRoot = new GameObject(name + "_Ghost").transform;

        foreach (Transform block in transform)
        {
            GameObject ghostBlock = Instantiate(block.gameObject, ghostRoot);

            Collider2D collider = ghostBlock.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;

            SpriteRenderer spriteRenderer = ghostBlock.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                spriteRenderer.color = new Color(c.r, c.g, c.b, 0.22f);
                spriteRenderer.sortingOrder = spriteRenderer.sortingOrder - 1;
            }
        }
    }

    void UpdateGhost()
    {
        if (ghostRoot == null) return;

        ghostRoot.position = transform.position;
        ghostRoot.rotation = transform.rotation;

        while (true)
        {
            ghostRoot.position += Vector3.down;

            if (!Board.IsValidPosition(ghostRoot))
            {
                ghostRoot.position += Vector3.up;
                break;
            }
        }
    }

    void AddToGrid()
    {
        foreach (Transform block in transform)
        {
            Vector2 pos = Board.Round(block.position);
            Board.grid[(int)pos.x, (int)pos.y] = block;
        }
    }
}