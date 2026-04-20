using UnityEngine;

public class MultiLayerParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        [Tooltip("Parent GameObject containing all sprites for this layer")]
        public GameObject layerParent;

        [Tooltip("All GameObjects (sprites) in this layer - add duplicates here")]
        public GameObject[] layerObjects;

        public float scrollSpeed = 1.0f;
        [Tooltip("Mark this layer as ground so respawn logic places tiles edge-to-edge")]
        public bool isGround = false;
        [Tooltip("Small overlap adjustment for ground tiles (negative to overlap slightly)")]
        public float groundOverlap = -0.01f;
    }

    [Header("Parallax Layers")]
    public ParallaxLayer[] layers;

    [Header("Settings")]
    public float globalSpeedMultiplier = 1.0f;

    [Header("Respawn Settings")]
    public float despawnX = -26f;

    [Tooltip("Gap between sprites when respawning (negative for overlap)")]
    public float respawnGap = 0f;

    void Start()
    {
        // nothing needed here; we use GameManager.Instance in Update
    }

    void Update()
    {
        // Respect global game state from GameManager (Flappy GameManager API)
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.IsStarted()) return;
        if (GameManager.Instance.IsGameOver()) return;

        foreach (ParallaxLayer layer in layers)
        {
            MoveLayer(layer);
        }
    }

    void MoveLayer(ParallaxLayer layer)
    {
        if (layer.layerObjects == null || layer.layerObjects.Length == 0) return;

        // Base multiplier for this layer
        float baseMultiplier = layer.scrollSpeed * globalSpeedMultiplier;
        // Prefer using pipe speed from GameManager so parallax matches other movers
        float worldSpeed = GameManager.Instance != null ? GameManager.Instance.GetPipeSpeed() : 0f;
        float speed = (worldSpeed > 0f ? worldSpeed * baseMultiplier : baseMultiplier) * Time.deltaTime;

        foreach (GameObject obj in layer.layerObjects)
        {
            if (obj == null) continue;

            // Move sprite
            obj.transform.position += Vector3.left * speed;

            // determine sprite half-width for edge checks
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
            float spriteWidth = spriteRenderer != null ? spriteRenderer.bounds.size.x : 20f;
            float halfWidth = spriteWidth * 0.5f;

            // If sprite's left edge went too far left, respawn it to the right
            float leftEdge = obj.transform.position.x - halfWidth;
            if (leftEdge < despawnX)
            {
                // Find the rightmost edge (center + half width) of the layer
                float rightmostEdge = GetRightmostEdgeInLayer(layer);

                float newCenterX;
                if (layer.isGround)
                {
                    // For ground tiles, place the new center so left edge = rightmostEdge + respawnGap
                    // rightmostEdge is center + half width of rightmost tile, so add full width of this tile
                    newCenterX = rightmostEdge + spriteWidth + respawnGap + layer.groundOverlap;
                }
                else
                {
                    // Set new center position so the left edge sits at rightmostEdge + gap
                    newCenterX = rightmostEdge + halfWidth + respawnGap;
                }

                obj.transform.position = new Vector3(newCenterX, obj.transform.position.y, obj.transform.position.z);
            }
        }
    }

    float GetRightmostXInLayer(ParallaxLayer layer)
    {
        float rightmostX = float.MinValue;

        foreach (GameObject obj in layer.layerObjects)
        {
            if (obj == null) continue;

            if (obj.transform.position.x > rightmostX)
            {
                rightmostX = obj.transform.position.x;
            }
        }

        return rightmostX;
    }

    // Returns the x coordinate of the rightmost edge (center + half width) among objects in the layer.
    float GetRightmostEdgeInLayer(ParallaxLayer layer)
    {
        float rightmost = float.MinValue;

        foreach (GameObject obj in layer.layerObjects)
        {
            if (obj == null) continue;

            float halfWidth = 10f;
            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null)
                halfWidth = sr.bounds.size.x * 0.5f;

            float edge = obj.transform.position.x + halfWidth;
            if (edge > rightmost) rightmost = edge;
        }

        return rightmost == float.MinValue ? 0f : rightmost;
    }
}
