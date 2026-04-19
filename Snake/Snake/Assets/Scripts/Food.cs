using UnityEngine;

public class Food : MonoBehaviour
{
    public FoodType foodType;
    void Update()
    {
        float scale = 3.5f + Mathf.Sin(Time.time * 5f) * 0.5f;
        transform.localScale = new Vector3(scale, scale, 8f);
    }
}

public enum FoodType
{
    Normal,
    SpeedBoost,
    Slow
}

