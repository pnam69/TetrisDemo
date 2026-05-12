using UnityEngine;

public class Food : MonoBehaviour
{
    public FoodType foodType;
    private float pulseSpeed = 5f;
    private float pulseAmount = 0.5f;
    private Color originalColor;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
        else
            originalColor = Color.white;

        // Set visual properties based on food type
        ApplyFoodTypeProperties();
    }

    void Update()
    {
        // Pulsing animation
        float scale = 3.5f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = new Vector3(scale, scale, 8f);

        // Color-based effects
        UpdateFoodVisuals();
    }

    private void ApplyFoodTypeProperties()
    {
        if (spriteRenderer == null) return;

        switch (foodType)
        {
            case FoodType.Normal:
                spriteRenderer.color = new Color(1f, 0.2f, 0.2f, 1f); // Red
                pulseSpeed = 5f;
                pulseAmount = 0.5f;
                break;

            case FoodType.SpeedBoost:
                spriteRenderer.color = new Color(1f, 0.8f, 0f, 1f); // Gold
                pulseSpeed = 8f; // Faster pulse
                pulseAmount = 0.7f; // More dramatic
                break;

            case FoodType.Slow:
                spriteRenderer.color = new Color(0.2f, 0.6f, 1f, 1f); // Blue
                pulseSpeed = 3f; // Slower pulse
                pulseAmount = 0.3f;
                break;
        }
    }

    private void UpdateFoodVisuals()
    {
        if (spriteRenderer == null) return;

        // Speed boost: glow effect
        if (foodType == FoodType.SpeedBoost)
        {
            float glow = Mathf.Sin(Time.time * 8f) * 0.3f + 0.7f;
            spriteRenderer.color = new Color(1f, 0.8f * glow, 0f, 1f);
        }
        // Slow: subtle dimming effect
        else if (foodType == FoodType.Slow)
        {
            float dim = Mathf.Cos(Time.time * 4f) * 0.2f + 0.8f;
            spriteRenderer.color = new Color(0.2f * dim, 0.6f * dim, 1f, 1f);
        }
    }

    public string GetFoodDescription()
    {
        return foodType switch
        {
            FoodType.Normal => "+10 points",
            FoodType.SpeedBoost => "+15 points, Speed ↑",
            FoodType.Slow => "+5 points, Speed ↓",
            _ => ""
        };
    }

    public int GetPointValue()
    {
        return foodType switch
        {
            FoodType.Normal => 10,
            FoodType.SpeedBoost => 15,
            FoodType.Slow => 5,
            _ => 0
        };
    }
}

public enum FoodType
{
    Normal,
    SpeedBoost,
    Slow
}

