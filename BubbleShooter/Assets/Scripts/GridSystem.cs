using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    [System.Serializable]
    public class LevelBubble
    {
        public Vector2Int position;
        public int colorID;
    }
    private struct SnapRequest
    {
        public int bubbleId;
        public Bubble bubble;
        public Vector2Int? anchor;
        public Vector2 hitPoint;
    }
    [SerializeField, Range(0.001f, 1f)]
    public float wallThickness = 0.05f;
    private Queue<SnapRequest> snapQueue = new();
    private HashSet<int> pendingSnapIds = new();
    private bool processing;
    public float cellSize = 1.2f;
    public int minMatchCount = 3;
    public GameObject bubblePrefab;
    public Color[] bubbleColors;
    public List<LevelBubble> initialLayout = new();
    public int defaultRows = 5;
    public int defaultCols = 8;
    public int defaultStartRow = 2;
    [Range(0.75f, 1f)] public float hexRowFactor = 0.866f;
    [Range(0f, 1f)] public float rowXOffset = 0.5f;
    [Range(2, 10)] public int defaultColorCount = 4;
    public Color wallVisualColor = new Color(0.85f, 0.8f, 1f, 1.0f);
    public Color boardBackgroundColor = new Color(0.08f, 0.1f, 0.16f, 0.05f);
    private static Sprite whiteSprite;

    public Dictionary<Vector2Int, Bubble> grid = new();
    public int TopRowThreshold
    {
        get
        {
            int top = 0;
            foreach (Vector2Int pos in grid.Keys)
            {
                if (pos.y > top) top = pos.y;
            }

            return top;
        }
    }

    void Start()
    {
        if (bubbleColors == null || bubbleColors.Length == 0)
        {
            bubbleColors = new[] { Color.red, Color.green, Color.blue, Color.yellow };
        }

        if (bubblePrefab == null)
        {
            Shooter shooter = FindObjectOfType<Shooter>();
            if (shooter != null)
            {
                bubblePrefab = shooter.bubblePrefab;
            }
        }


        if (cellSize <= 0.01f)
        {
            AutoConfigureCellSize();
        }
        EnsureTopCollider();
        EnsureWallVisuals();
        EnsureBoardVisual();

        LoadLayout();
    }

    void AutoConfigureCellSize()
    {
        SpriteRenderer sr = bubblePrefab.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float scaleX = Mathf.Abs(bubblePrefab.transform.localScale.x);
        float spriteWidth = sr.sprite.bounds.size.x * scaleX;
        if (spriteWidth > 0.01f)
        {
            cellSize = spriteWidth;
        }
    }

    void EnsureWallVisuals()
    {
        ApplyVisualToTaggedCollider("Wall");
        ApplyVisualToTaggedCollider("Top");
    }

    void EnsureBoardVisual()
    {
        Transform existing = transform.Find("__BoardVisual");
        GameObject go = existing != null ? existing.gameObject : new GameObject("__BoardVisual");
        if (existing == null) go.transform.SetParent(transform, false);

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();

        sr.sprite = GetWhiteSprite();
        sr.color = boardBackgroundColor;
        sr.sortingOrder = -30;

        float width = Mathf.Max(1f, defaultCols * cellSize + cellSize);
        float height = Mathf.Max(1f, (defaultRows + defaultStartRow + 1) * (cellSize * hexRowFactor));
        go.transform.localScale = new Vector3(width, height, 1f);
        go.transform.localPosition = new Vector3(0f, height * 0.5f - (cellSize * hexRowFactor), 0f);
    }

    void ApplyVisualToTaggedCollider(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject go = objects[i];
            if (go == null) continue;

            BoxCollider2D col = go.GetComponent<BoxCollider2D>();
            if (col == null) continue;

            Transform visual = go.transform.Find("__WallVisual");
            GameObject visualGo;
            if (visual == null)
            {
                visualGo = new GameObject("__WallVisual");
                visualGo.transform.SetParent(go.transform, false);
            }
            else
            {
                visualGo = visual.gameObject;
            }

            SpriteRenderer sr = visualGo.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                sr = visualGo.AddComponent<SpriteRenderer>();
            }

            sr.sprite = GetWhiteSprite();
            sr.color = wallVisualColor;
            sr.sortingOrder = -20;

            visualGo.transform.localPosition = col.offset;
            visualGo.transform.localScale = new Vector3(
                wallThickness,
                col.size.y,
                1f
            );
        }
    }

    static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null) return whiteSprite;

        Texture2D tex = Texture2D.whiteTexture;
        whiteSprite = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
        return whiteSprite;
    }

    void EnsureTopCollider()
    {
        GameObject top = GameObject.FindGameObjectWithTag("Top");
        if (top == null) return;

        BoxCollider2D topCollider = top.GetComponent<BoxCollider2D>();
        //if (topCollider == null)
        //{
        //    topCollider = top.AddComponent<BoxCollider2D>();
        //    topCollider.size = new Vector2(10f, 0.5f);
        //}
    }

    private static readonly Vector2Int[] EvenRowNeighbors =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, -1)
    };

    private static readonly Vector2Int[] OddRowNeighbors =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 1),
        new Vector2Int(0, 1),
        new Vector2Int(1, -1),
        new Vector2Int(0, -1)
    };

    public Vector2 GridToWorld(Vector2Int pos)
    {
        float width = cellSize;
        float height = cellSize * hexRowFactor;

        float xOffset = (pos.y % 2 == 0) ? 0f : width * 0.5f;

        return new Vector2(
            pos.x * width + xOffset,
            pos.y * height
        );
    }

    public Vector2Int WorldToGrid(Vector2 world)
    {
        int y = Mathf.RoundToInt(world.y / (hexRowFactor * cellSize));
        float xOffset = (y % 2 == 0) ? 0f : rowXOffset;
        int x = Mathf.RoundToInt(world.x / cellSize - xOffset);

        return new Vector2Int(x, y);
    }

    private IEnumerable<Vector2Int> GetNeighbors(Vector2Int pos)
    {
        Vector2Int[] offsets = (pos.y % 2 == 0) ? EvenRowNeighbors : OddRowNeighbors;
        for (int i = 0; i < offsets.Length; i++)
        {
            yield return pos + offsets[i];
        }
    }

    public void RequestSnap(Bubble bubble, Vector2Int? anchor, Vector2 hitPoint)
    {
        if (bubble == null) return;

        int id = bubble.GetInstanceID();
        if (pendingSnapIds.Contains(id)) return;
        pendingSnapIds.Add(id);

        snapQueue.Enqueue(new SnapRequest
        {
            bubbleId = id,
            bubble = bubble,
            anchor = anchor,
            hitPoint = hitPoint
        });

        if (!processing)
            StartCoroutine(ProcessSnapQueue());
    }

    private System.Collections.IEnumerator ProcessSnapQueue()
    {
        processing = true;

        while (snapQueue.Count > 0)
        {
            yield return new WaitForFixedUpdate();

            SnapRequest item = snapQueue.Dequeue();
            pendingSnapIds.Remove(item.bubbleId);
            if (item.bubble == null) continue;

            ExecuteSnap(item.bubble, item.anchor, item.hitPoint);
        }

        processing = false;
    }

    private void ExecuteSnap(Bubble bubble, Vector2Int? anchor, Vector2 hitPoint)
    {
        Vector2Int center = anchor ?? WorldToGrid(hitPoint);
        Vector2Int target = FindSnapTarget(center, hitPoint);

        if (target.y < defaultStartRow)
            target.y = defaultStartRow;

        if (grid.ContainsKey(target))
        {
            target = FindNearestFreeCell(center, hitPoint);
        }

        if (grid.ContainsKey(target))
        {
            Destroy(bubble.gameObject);
            return;
        }

        foreach (var kv in grid)
        {
            if (kv.Value == bubble)
                return;
        }

        bubble.StopAndLock();
        bubble.isSnapped = true;

        // assign grid state first
        grid[target] = bubble;

        // then move visual
        bubble.transform.position = GridToWorld(target);
        bubble.gridPos = target;

        ResolveBoardAfterSnap(target);
    }

    private Vector2Int FindNearestFreeCell(Vector2Int start, Vector2 hitPoint)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> depth = new Dictionary<Vector2Int, int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        q.Enqueue(start);
        visited.Add(start);
        depth[start] = 0;

        int bestDepth = int.MaxValue;
        List<Vector2Int> candidates = new List<Vector2Int>();

        while (q.Count > 0)
        {
            Vector2Int current = q.Dequeue();
            int d = depth[current];
            if (d > bestDepth) continue;

            if (!grid.ContainsKey(current))
            {
                if (d < bestDepth)
                {
                    bestDepth = d;
                    candidates.Clear();
                }

                if (d == bestDepth)
                    candidates.Add(current);

                continue;
            }

            foreach (Vector2Int n in GetNeighbors(current))
            {
                if (visited.Contains(n)) continue;
                visited.Add(n);
                depth[n] = d + 1;
                q.Enqueue(n);
            }
        }

        if (candidates.Count == 0)
            return start;

        Vector2Int best = candidates[0];
        float bestSqr = (GridToWorld(best) - hitPoint).sqrMagnitude;
        for (int i = 1; i < candidates.Count; i++)
        {
            float sqr = (GridToWorld(candidates[i]) - hitPoint).sqrMagnitude;
            if (sqr < bestSqr)
            {
                best = candidates[i];
                bestSqr = sqr;
            }
        }

        return best;
    }

    private Vector2Int FindSnapTarget(Vector2Int center, Vector2 hitPoint)
    {
        if (!grid.ContainsKey(center))
            return center;

        // Bubble-shooter style: BFS through occupied cells to nearest free slot,
        // then pick the one closest to the collision point.
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        Dictionary<Vector2Int, int> depth = new Dictionary<Vector2Int, int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        q.Enqueue(center);
        visited.Add(center);
        depth[center] = 0;

        int bestDepth = int.MaxValue;
        List<Vector2Int> candidates = new List<Vector2Int>();

        while (q.Count > 0)
        {
            Vector2Int current = q.Dequeue();
            int d = depth[current];
            if (d > bestDepth) continue;

            foreach (Vector2Int n in GetNeighbors(current))
            {
                if (visited.Contains(n)) continue;
                visited.Add(n);

                int nd = d + 1;

                if (!grid.ContainsKey(n))
                {
                    if (nd < bestDepth)
                    {
                        bestDepth = nd;
                        candidates.Clear();
                    }

                    if (nd == bestDepth)
                        candidates.Add(n);

                    continue;
                }

                depth[n] = nd;
                q.Enqueue(n);
            }
        }

        if (candidates.Count == 0)
            return center;

        Vector2Int best = candidates[0];
        float bestSqr = (GridToWorld(best) - hitPoint).sqrMagnitude;

        for (int i = 1; i < candidates.Count; i++)
        {
            float sqr = (GridToWorld(candidates[i]) - hitPoint).sqrMagnitude;
            if (sqr < bestSqr)
            {
                best = candidates[i];
                bestSqr = sqr;
            }
        }

        return best;
    }

    public void LoadLayout()
    {
        if (bubblePrefab == null)
        {
            Debug.LogWarning("GridSystem: Missing bubblePrefab, cannot spawn grid.");
            return;
        }

        if (initialLayout == null || initialLayout.Count == 0)
        {
            BuildDefaultLayout();
        }

        foreach (KeyValuePair<Vector2Int, Bubble> kv in grid)
        {
            if (kv.Value != null)
            {
                Destroy(kv.Value.gameObject);
            }
        }

        grid.Clear();

        for (int i = 0; i < initialLayout.Count; i++)
        {
            LevelBubble cell = initialLayout[i];
            Vector2 worldPos = GridToWorld(cell.position);
            GameObject go = Instantiate(bubblePrefab, worldPos, Quaternion.identity);
            Bubble bubble = go.GetComponent<Bubble>();
            if (bubble == null) continue;

            bubble.gridPos = cell.position;
            bubble.StopAndLock();

            if (bubbleColors != null && bubbleColors.Length > 0)
            {
                int colorIndex = Mathf.Clamp(cell.colorID, 0, bubbleColors.Length - 1);
                bubble.SetColor(colorIndex, bubbleColors[colorIndex]);
            }
            else
            {
                int colorIndex = Mathf.Abs(cell.colorID) % 4;
                Color fallback = colorIndex switch
                {
                    0 => Color.red,
                    1 => Color.green,
                    2 => Color.blue,
                    _ => Color.yellow
                };
                bubble.SetColor(colorIndex, fallback);
            }

            grid[cell.position] = bubble;
        }
    }

    void BuildDefaultLayout()
    {
        initialLayout = new List<LevelBubble>();
        int colorsCount = (bubbleColors != null && bubbleColors.Length > 0)
            ? Mathf.Min(bubbleColors.Length, defaultColorCount)
            : defaultColorCount;
        int startRow = Mathf.Max(0, defaultStartRow);
        int xStart = -Mathf.FloorToInt(defaultCols * 0.5f);

        for (int y = defaultRows - 1; y >= 0; y--)
        {
            for (int x = 0; x < defaultCols; x++)
            {
                initialLayout.Add(new LevelBubble
                {
                    position = new Vector2Int(xStart + x, y + startRow),
                    colorID = (x + y) % colorsCount
                });
            }
        }
    }

    public Vector3 GetBoardCenterWorld()
    {
        if (grid.Count == 0)
        {
            float approxX = ((defaultCols - 1) * 0.5f) * cellSize;
            float approxY = ((defaultStartRow + defaultRows) * 0.5f) * hexRowFactor * cellSize;
            return new Vector3(approxX, approxY, 0f);
        }

        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;

        foreach (Vector2Int pos in grid.Keys)
        {
            Vector2 w = GridToWorld(pos);
            if (w.x < minX) minX = w.x;
            if (w.x > maxX) maxX = w.x;
            if (w.y < minY) minY = w.y;
            if (w.y > maxY) maxY = w.y;
        }

        return new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
    }

    private void ResolveBoardAfterSnap(Vector2Int origin)
    {
        GameManager.Instance?.SetResolving(true);
        try
        {
            HashSet<Vector2Int> matched = GetConnectedSameColor(origin);
            if (matched.Count < minMatchCount)
            {
                GameManager.Instance?.OnShotResolved(0, 0);
                return;
            }

            int poppedCount = RemoveBubbles(matched);
            if (poppedCount > 0)
            {
                AudioManager.Instance?.PlayPop();
            }

            HashSet<Vector2Int> anchored = GetAnchoredBubbles();
            List<Vector2Int> floating = new();
            foreach (Vector2Int pos in grid.Keys)
            {
                if (!anchored.Contains(pos))
                {
                    floating.Add(pos);
                }
            }

            int droppedCount = RemoveBubbles(floating);
            if (droppedCount > 0)
            {
                AudioManager.Instance?.PlayDrop();
            }

            GameManager.Instance?.OnShotResolved(poppedCount, droppedCount);
        }
        finally
        {
            GameManager.Instance?.SetResolving(false);
        }
    }

    private HashSet<Vector2Int> GetConnectedSameColor(Vector2Int start)
    {
        HashSet<Vector2Int> result = new();
        if (!grid.TryGetValue(start, out Bubble startBubble) || startBubble == null)
        {
            return result;
        }

        int targetColor = startBubble.colorID;
        Queue<Vector2Int> queue = new();
        queue.Enqueue(start);
        result.Add(start);

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (Vector2Int next in GetNeighbors(current))
            {
                if (result.Contains(next)) continue;
                if (!grid.TryGetValue(next, out Bubble nextBubble) || nextBubble == null) continue;
                if (nextBubble.colorID != targetColor) continue;

                result.Add(next);
                queue.Enqueue(next);
            }
        }

        return result;
    }

    private HashSet<Vector2Int> GetAnchoredBubbles()
    {
        HashSet<Vector2Int> anchored = new();
        Queue<Vector2Int> queue = new();

        foreach (KeyValuePair<Vector2Int, Bubble> kv in grid)
        {
            if (kv.Key.y >= TopRowThreshold)
            {
                anchored.Add(kv.Key);
                queue.Enqueue(kv.Key);
            }
        }

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (Vector2Int next in GetNeighbors(current))
            {
                if (anchored.Contains(next)) continue;
                if (!grid.ContainsKey(next)) continue;

                anchored.Add(next);
                queue.Enqueue(next);
            }
        }

        return anchored;
    }

    private int RemoveBubbles(IEnumerable<Vector2Int> positions)
    {
        List<Vector2Int> toRemove = new();
        foreach (Vector2Int pos in positions)
        {
            if (grid.ContainsKey(pos))
            {
                toRemove.Add(pos);
            }
        }

        for (int i = 0; i < toRemove.Count; i++)
        {
            Vector2Int pos = toRemove[i];
            Bubble bubble = grid[pos];
            grid.Remove(pos);
            if (bubble != null)
            {
                Destroy(bubble.gameObject);
            }
        }

        return toRemove.Count;
    }

}