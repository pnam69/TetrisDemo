using UnityEngine;

public class DestroyOffscreen : MonoBehaviour
{
    public float destroyX = -15f;

    void Update()
    {
        if (transform.position.x < destroyX)
        {
            Destroy(gameObject);
        }
    }
}