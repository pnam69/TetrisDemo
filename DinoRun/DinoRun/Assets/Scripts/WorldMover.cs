using UnityEngine;

public class WorldMover : MonoBehaviour
{
    void Update()
    {
        if(GameManager.Instance == null) return;
        if (GameManager.Instance.isGameOver) return;
        transform.position += Vector3.left * GameManager.Instance.worldSpeed * Time.deltaTime;
    }
}