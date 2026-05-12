using UnityEngine;

public class MultiLayerParallax : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayer
    {
        public GameObject layerParent;
        public GameObject[] layerObjects;
        public float scrollSpeed = 1.0f;

        [Header("Day/Night")]
        [Range(0f, 1f)]
        public float darknessMultiplier = 1f;
    }

    public ParallaxLayer[] layers;
    public float globalSpeedMultiplier = 1.0f;

    public float despawnX = -26f;
    public float respawnGap = 0f;

    void Update()
    {
        if (GameManager.Instance == null) return;

        UpdateDayNight();

        if (!GameManager.Instance.gameStarted) return;
        if (GameManager.Instance.isGameOver) return;

        foreach (ParallaxLayer layer in layers)
        {
            MoveLayer(layer);
        }
    }

    void UpdateDayNight()
    {
        float duration = GameManager.Instance.dayNightCycleDuration;

        if (duration <= 0f) return;

        float t = Mathf.PingPong(
            Time.time / duration,
            1f
        );

        Color tint = Color.Lerp(
            Color.white,
            new Color(0.4f, 0.4f, 0.6f),
            t
        );

        // Tint parallax layers
        foreach (ParallaxLayer layer in layers)
        {
            foreach (GameObject obj in layer.layerObjects)
            {
                if (obj == null) continue;

                SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
                if (sr == null) continue;

                sr.color = Color.Lerp(
                    Color.white,
                    tint,
                    layer.darknessMultiplier
                );
            }
        }

        // Tint player
        ApplyTagTint("Player", tint);

        // Tint obstacles
        ApplyTagTint("Obstacle", tint);
    }
    void ApplyTagTint(string tagName, Color tint)
    {
        GameObject[] objects =
            GameObject.FindGameObjectsWithTag(tagName);

        foreach (GameObject obj in objects)
        {
            SpriteRenderer[] renderers =
                obj.GetComponentsInChildren<SpriteRenderer>();

            foreach (SpriteRenderer sr in renderers)
            {
                sr.color = tint;
            }
        }
    }

    void MoveLayer(ParallaxLayer layer)
    {
        if (layer.layerObjects == null || layer.layerObjects.Length == 0)
            return;

        float baseMultiplier =
            layer.scrollSpeed * globalSpeedMultiplier;

        float worldSpeed =
            GameManager.Instance.worldSpeed;

        float speed =
            worldSpeed * baseMultiplier * Time.deltaTime;

        foreach (GameObject obj in layer.layerObjects)
        {
            if (obj == null) continue;

            obj.transform.position += Vector3.left * speed;

            if (obj.transform.position.x < despawnX)
            {
                float rightmostEdge =
                    GetRightmostEdgeInLayer(layer);

                SpriteRenderer sr =
                    obj.GetComponent<SpriteRenderer>();

                float width =
                    sr != null
                    ? sr.bounds.size.x
                    : 20f;

                float newCenterX =
                    rightmostEdge +
                    (width * 0.5f) +
                    respawnGap;

                obj.transform.position =
                    new Vector3(
                        newCenterX,
                        obj.transform.position.y,
                        obj.transform.position.z
                    );
            }
        }
    }

    float GetRightmostEdgeInLayer(
        ParallaxLayer layer)
    {
        float rightmost = float.MinValue;

        foreach (GameObject obj in layer.layerObjects)
        {
            if (obj == null) continue;

            SpriteRenderer sr =
                obj.GetComponent<SpriteRenderer>();

            float halfWidth =
                sr != null
                ? sr.bounds.size.x * 0.5f
                : 10f;

            float edge =
                obj.transform.position.x +
                halfWidth;

            if (edge > rightmost)
                rightmost = edge;
        }

        return rightmost;
    }
}