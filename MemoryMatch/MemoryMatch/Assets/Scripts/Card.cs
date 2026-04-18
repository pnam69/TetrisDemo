using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int cardID;
    public Image frontImage;
    public Sprite[] faceSprites;
    public CanvasGroup backImage;
    public LayoutElement layoutElement;

    private bool isFlipped = false;
    private bool isMatched = false;

    public bool IsMatched => isMatched;
    public bool IsFlipped => isFlipped;

    private void Awake()
    {
        if (layoutElement == null)
            layoutElement = GetComponent<LayoutElement>();
    }

    public void SetSize(Vector2 size)
    {
        if (layoutElement == null) return;

        layoutElement.preferredWidth = size.x;
        layoutElement.preferredHeight = size.y;
    }

    public void Flip()
    {
        if (isFlipped || isMatched) return;
        if (backImage == null) return;
        if (!GameManager.Instance.CanSelect()) return;

        isFlipped = true;
        StartCoroutine(FlipAnimation(true));
        backImage.blocksRaycasts = false;
        backImage.interactable = false;

        GameManager.Instance.PlayFlip();
        GameManager.Instance.CardSelected(this);
    }
    private IEnumerator FlipAnimation(bool showFront)
    {
        float duration = 0.15f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = new Vector3(
                Mathf.Lerp(1f, 0f, time / duration),
                1f,
                1f
            );
            yield return null;
        }

        backImage.alpha = showFront ? 0f : 1f;

        time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            transform.localScale = new Vector3(
                Mathf.Lerp(0f, 1f, time / duration),
                1f,
                1f
            );
            yield return null;
        }
    }

    public void Hide()
    {
        isFlipped = false;
        if (backImage == null) return;

        StartCoroutine(FlipAnimation(false));
        backImage.blocksRaycasts = true;
        backImage.interactable = true;
    }

    public void Match()
    {
        isMatched = true;
        isFlipped = true;
        if (backImage == null) return;

        if (frontImage != null)
        {
            frontImage.color = new Color(1f, 1f, 1f, 0.7f);
        }
        backImage.blocksRaycasts = false;
        backImage.interactable = false;
        GameManager.Instance.PlayMatch();
    }
    
    public void RevealTemporary(float duration)
    {
        StartCoroutine(RevealTempRoutine(duration));
    }

    private IEnumerator RevealTempRoutine(float duration)
    {
        if (isMatched || isFlipped) yield break;

        isFlipped = true;
        if (backImage != null)
        {
            backImage.alpha = 0f;
            backImage.blocksRaycasts = false;
            backImage.interactable = false;
        }

        yield return new WaitForSeconds(duration);

        if (!isMatched)
        {
            isFlipped = false;
            if (backImage != null)
            {
                backImage.alpha = 1f;
                backImage.blocksRaycasts = true;
                backImage.interactable = true;
            }
        }
    }
    public void SetCard(int id)
    {
        cardID = id;

        if (frontImage != null && faceSprites != null && id < faceSprites.Length)
        {
            frontImage.sprite = faceSprites[id];
        }
    }
}