using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnimationHelper : MonoBehaviour
{
    public static UIAnimationHelper Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Scale pop animation - element scales up and fades out
    /// </summary>
    public void PlayScalePopAnimation(Transform target, float duration = 0.5f, float targetScale = 1.5f)
    {
        StartCoroutine(ScalePopCoroutine(target, duration, targetScale));
    }

    private IEnumerator ScalePopCoroutine(Transform target, float duration, float targetScale)
    {
        Vector3 startScale = target.localScale;
        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = target.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            target.localScale = Vector3.Lerp(startScale, startScale * targetScale, t);
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        target.localScale = startScale;
        canvasGroup.alpha = 1f;
    }

    /// <summary>
    /// Bounce animation - element bounces up and down
    /// </summary>
    public void PlayBounceAnimation(Transform target, float duration = 0.6f, float bounceHeight = 0.5f)
    {
        StartCoroutine(BounceCoroutine(target, duration, bounceHeight));
    }

    private IEnumerator BounceCoroutine(Transform target, float duration, float bounceHeight)
    {
        Vector3 startPos = target.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Parabolic bounce
            float height = Mathf.Sin(t * Mathf.PI) * bounceHeight;
            target.position = startPos + Vector3.up * height;

            yield return null;
        }

        target.position = startPos;
    }

    /// <summary>
    /// Shake animation - element shakes horizontally
    /// </summary>
    public void PlayShakeAnimation(Transform target, float duration = 0.3f, float intensity = 0.1f)
    {
        StartCoroutine(ShakeCoroutine(target, duration, intensity));
    }

    private IEnumerator ShakeCoroutine(Transform target, float duration, float intensity)
    {
        Vector3 startPos = target.position;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float shake = (Random.value - 0.5f) * intensity * 2f;
            target.position = startPos + Vector3.right * shake;

            yield return null;
        }

        target.position = startPos;
    }

    /// <summary>
    /// Flash animation - element flashes with color
    /// </summary>
    public void PlayFlashAnimation(Image target, Color flashColor, float duration = 0.3f, int flashes = 2)
    {
        StartCoroutine(FlashCoroutine(target, flashColor, duration, flashes));
    }

    private IEnumerator FlashCoroutine(Image target, Color flashColor, float duration, int flashes)
    {
        Color originalColor = target.color;
        float flashDuration = duration / flashes;

        for (int i = 0; i < flashes; i++)
        {
            float elapsed = 0;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                target.color = Color.Lerp(originalColor, flashColor, t);
                yield return null;
            }

            elapsed = 0;
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flashDuration;
                target.color = Color.Lerp(flashColor, originalColor, t);
                yield return null;
            }
        }

        target.color = originalColor;
    }

    /// <summary>
    /// Slide in animation from side
    /// </summary>
    public void PlaySlideInAnimation(RectTransform target, Vector2 fromPosition, Vector2 toPosition, float duration = 0.5f)
    {
        StartCoroutine(SlideInCoroutine(target, fromPosition, toPosition, duration));
    }

    private IEnumerator SlideInCoroutine(RectTransform target, Vector2 fromPos, Vector2 toPos, float duration)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Ease out cubic
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            target.anchoredPosition = Vector2.Lerp(fromPos, toPos, easeT);

            yield return null;
        }

        target.anchoredPosition = toPos;
    }

    /// <summary>
    /// Fade in/out animation
    /// </summary>
    public void PlayFadeAnimation(CanvasGroup target, float fromAlpha, float toAlpha, float duration = 0.3f)
    {
        StartCoroutine(FadeCoroutine(target, fromAlpha, toAlpha, duration));
    }

    private IEnumerator FadeCoroutine(CanvasGroup target, float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            yield return null;
        }

        target.alpha = toAlpha;
    }

    /// <summary>
    /// Rotate animation - element rotates
    /// </summary>
    public void PlayRotateAnimation(Transform target, float targetRotation, float duration = 0.5f)
    {
        StartCoroutine(RotateCoroutine(target, targetRotation, duration));
    }

    private IEnumerator RotateCoroutine(Transform target, float targetRotation, float duration)
    {
        Quaternion startRotation = target.rotation;
        Quaternion targetQuat = Quaternion.Euler(0, 0, targetRotation);
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            target.rotation = Quaternion.Lerp(startRotation, targetQuat, t);

            yield return null;
        }

        target.rotation = targetQuat;
    }
}
