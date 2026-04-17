using UnityEngine;

public class SwipeInput : MonoBehaviour
{
    public SnakeController snake;

    private Vector2 startTouch;
    private Vector2 endTouch;

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
            startTouch = touch.position;

        if (touch.phase == TouchPhase.Ended)
        {
            endTouch = touch.position;
            DetectSwipe();
        }
    }

    void DetectSwipe()
    {
        Vector2 delta = endTouch - startTouch;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0)
                snake.SetDirection(Vector2Int.right);
            else
                snake.SetDirection(Vector2Int.left);
        }
        else
        {
            if (delta.y > 0)
                snake.SetDirection(Vector2Int.up);
            else
                snake.SetDirection(Vector2Int.down);
        }
    }
}