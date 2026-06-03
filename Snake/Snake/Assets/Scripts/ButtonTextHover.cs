using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonTextHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public RectTransform textTransform;

    [Header("Text Position")]
    public float normalY = 0f;
    public float hoverY = 5f;
    public float pressedY = -3f;

    private bool isHovering;
    private Vector2 startPos;
    private bool initialized = false;

    void Start()
    {
        if (textTransform == null)
            textTransform = GetComponentInChildren<TMP_Text>().rectTransform;

        startPos = textTransform.anchoredPosition;
        initialized = true;
        SetY(normalY);
    }
    
    void OnEnable()
    {
        if (initialized)
        {
            isHovering = false;
            SetY(normalY);
        }
    }

    void OnDisable()
    {
        isHovering = false;
        if (initialized && textTransform != null)
        {
            SetY(normalY);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        SetY(hoverY);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        SetY(normalY);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetY(pressedY);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        SetY(isHovering ? hoverY : normalY);
    }

    private void SetY(float offsetY)
    {
        textTransform.anchoredPosition =
            new Vector2(startPos.x, startPos.y + offsetY);
    }
}