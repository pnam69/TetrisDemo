using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public float cellSize = 1f;

    public Dictionary<Vector2Int, Bubble> grid = new();
    public Vector2 GridToWorld(Vector2Int pos)
    {
        float xOffset = (pos.y % 2 == 0) ? 0 : 0.5f;
        return new Vector2((pos.x + xOffset) * cellSize,
                           pos.y * 0.866f * cellSize);
    }

    public Vector2Int WorldToGrid(Vector2 world)
    {
        int y = Mathf.RoundToInt(world.y / (0.866f * cellSize));
        float xOffset = (y % 2 == 0) ? 0 : 0.5f;
        int x = Mathf.RoundToInt(world.x / cellSize - xOffset);

        return new Vector2Int(x, y);
    }
    public void SnapBubble(Bubble bubble)
    {
        Vector2Int gridPos = WorldToGrid(bubble.transform.position);

        Vector2 worldPos = GridToWorld(gridPos);
        bubble.transform.position = worldPos;

        bubble.gridPos = gridPos;
        bubble.StopAndLock();

        grid[gridPos] = bubble;
    }
}