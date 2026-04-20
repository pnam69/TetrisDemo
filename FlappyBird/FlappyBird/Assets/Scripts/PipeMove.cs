using UnityEngine;

public class PipeMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2.8f;
    
    [Header("Cleanup")]
    public float deadZoneX = -12f;
    
    private LogicScript logic;

    void Start()
    {
        GameObject logicObject = GameObject.FindGameObjectWithTag("Logic");
        if (logicObject != null)
        {
            logic = logicObject.GetComponent<LogicScript>();
        }
    }

    void Update()
    {
        if (logic != null && logic.IsGameOver())
        {
            return;
        }

        // Use the PipeMove's configured speed as a minimum so pipes aren't slower than expected.
        float currentSpeed = moveSpeed;
        if (GameManager.Instance != null)
        {
            // GameManager provides a dynamic world/pipe speed; ensure we don't go below the local moveSpeed
            currentSpeed = Mathf.Max(moveSpeed, GameManager.Instance.GetPipeSpeed());
        }

        transform.position += Vector3.left * currentSpeed * Time.deltaTime;
        
        if (transform.position.x < deadZoneX)
        {
            Destroy(gameObject);
        }
    }
}
