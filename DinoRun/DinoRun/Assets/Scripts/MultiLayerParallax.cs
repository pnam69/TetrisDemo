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
        // Respect global game state from GameManager
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.gameStarted) return;
        if (GameManager.Instance.isGameOver) return;

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
        // Prefer using worldSpeed when available so parallax matches other movers
        float worldSpeed = GameManager.Instance != null ? GameManager.Instance.worldSpeed : 0f;
        float speed = (worldSpeed > 0f ? worldSpeed * baseMultiplier : baseMultiplier) * Time.deltaTime;

        foreach (GameObject obj in layer.layerObjects)
        {
            if (obj == null) continue;

            // Move sprite
            obj.transform.position += Vector3.left * speed;

            // If sprite went too far left, respawn it to the right
            if (obj.transform.position.x < despawnX)
            {
                // Find the rightmost edge (center + half width) of the layer
                float rightmostEdge = GetRightmostEdgeInLayer(layer);

                // Get sprite width
                SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
                float spriteWidth = spriteRenderer != null ? spriteRenderer.bounds.size.x : 20f;

                // Set new center position so the left edge sits at rightmostEdge + gap
                float newCenterX = rightmostEdge + (spriteWidth * 0.5f) + respawnGap;

                obj.transform.position = new Vector3(
                    newCenterX,
                    obj.transform.position.y,
                    obj.transform.position.z
                );
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
