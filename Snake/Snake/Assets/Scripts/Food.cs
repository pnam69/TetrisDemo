using UnityEngine;

public class Food : MonoBehaviour
{
    public FoodType foodType;
    void Update()
    {
        float scale = 8f + Mathf.Sin(Time.time * 5f) * 1.1f;
        transform.localScale = new Vector3(scale, scale, 8f);
    }
}

public enum FoodType
{
    Normal,
    SpeedBoost,
    Slow
}

