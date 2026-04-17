using UnityEngine;

public class Food : MonoBehaviour
{
    public FoodType foodType;
}

public enum FoodType
{
    Normal,
    SpeedBoost,
    Slow
}