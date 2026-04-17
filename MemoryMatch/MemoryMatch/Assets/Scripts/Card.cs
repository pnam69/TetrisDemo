using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int cardID;
    public GameObject backImage;

    public bool isFlipped = false;
    public bool isMatched = false;

    public void Flip()
    {
        if (isFlipped || isMatched) return;

        if (GameManager.Instance == null || !GameManager.Instance.CanSelect())
            return;

        isFlipped = true;
        backImage.SetActive(false);

        GameManager.Instance.CardSelected(this);
    }
    public void Hide()
    {
        isFlipped = false;
        backImage.SetActive(true);
    }
    public void Match()
    {
        isMatched = true;
        isFlipped = true;
        if (backImage != null)
            backImage.SetActive(false);
    }
}