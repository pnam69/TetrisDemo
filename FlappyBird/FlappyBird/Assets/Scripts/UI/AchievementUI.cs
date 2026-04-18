using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class AchievementUI : MonoBehaviour
{
    public TMP_Text titleText;
    public Image icon;
    public float displayDuration = 2.2f;
    public Vector2 startOffset = new Vector2(0, -120);
    public Vector2 endOffset = new Vector2(0, -40);
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup canvasGroup;
    private RectTransform rect;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        // keep the panel active but hidden so coroutines can always run
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        // ensure we have a text target
        if (titleText == null)
        {
            titleText = GetComponentInChildren<TMP_Text>();
        }

        if (titleText != null)
        {
            // clear any default placeholder so the first show displays correctly
            titleText.text = string.Empty;
        }
    }

    public void Show(string message)
    {
        StopAllCoroutines();
        if (titleText != null)
            titleText.text = message;
        else
            Debug.LogWarning("AchievementUI: titleText not assigned and no TMP_Text child found.");

        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        // make visible for animation
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        rect.anchoredPosition = startOffset;

        // fade/slide in
        float t = 0f;
        float dur = 0.28f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = ease.Evaluate(Mathf.Clamp01(t / dur));
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, p);
            rect.anchoredPosition = Vector2.Lerp(startOffset, endOffset, p);
            yield return null;
        }

        // hold
        yield return new WaitForSecondsRealtime(displayDuration);

        // fade out
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = ease.Evaluate(Mathf.Clamp01(t / dur));
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, p);
            rect.anchoredPosition = Vector2.Lerp(endOffset, startOffset, p);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }
}
