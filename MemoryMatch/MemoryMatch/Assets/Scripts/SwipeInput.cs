using UnityEngine;

public class SwipeInput : MonoBehaviour
{

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
        }
        else
        {
        }
    }
}