using UnityEngine;

public class GroundLoop : MonoBehaviour
{
    private float width;

    void Start()
    {
        width = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        if (transform.position.x < -width)
        {
            transform.position += Vector3.right * width * 2f;
        }
    }
}