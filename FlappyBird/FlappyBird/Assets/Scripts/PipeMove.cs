using UnityEngine;

public class PipeMove : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    
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

        float currentSpeed = moveSpeed;
        if (GameManager.Instance != null)
        {
            currentSpeed = GameManager.Instance.GetPipeSpeed();
        }
        
        transform.position += Vector3.left * currentSpeed * Time.deltaTime;
        
        if (transform.position.x < deadZoneX)
        {
            Destroy(gameObject);
        }
    }
}
