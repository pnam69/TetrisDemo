using UnityEngine;

public class TriggerScript : MonoBehaviour
{
    private LogicScript logicScript;
    private bool hasScored = false;

    void Start()
    {
        GameObject logicObject = GameObject.FindGameObjectWithTag("Logic");
        if (logicObject != null)
        {
            logicScript = logicObject.GetComponent<LogicScript>();
        }
        else
        {
            Debug.LogError("Logic GameObject not found! Make sure there's a GameObject tagged 'Logic'.");
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasScored || logicScript == null) return;
        
        if (collision.CompareTag("Bird"))
        {
            hasScored = true;
            logicScript.addScore(1);
        }
    }
}
