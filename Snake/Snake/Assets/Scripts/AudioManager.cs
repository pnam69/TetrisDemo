using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sources")]
    public AudioSource sfxSource;
    public AudioSource bgmSource;

    [Header("Snake Clips")]
    public AudioClip bgmClip;
    public AudioClip uiClip;
    public AudioClip foodClip;
    public AudioClip speedBoostClip;
    public AudioClip slowClip;
    public AudioClip levelUpClip;
    public AudioClip gameOverClip;

    [Header("SFX Variation")]
    [Range(0.8f, 1.2f)] public float sfxPitchMin = 0.98f;
    [Range(0.8f, 1.2f)] public float sfxPitchMax = 1.02f;

    private bool isSoundEnabled = true;
    private bool isBgmPaused = false;
    private Coroutine bgmFadeRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
        }
    }

    void Start()
    {
        isSoundEnabled = PlayerPrefs.GetInt("Sound", 1) == 1;
        SetSoundEnabled(isSoundEnabled);
        PlayBgm();
    }

    public void PlayBgm()
    {
        if (!isSoundEnabled || bgmSource == null || bgmClip == null) return;
        if (bgmSource.isPlaying) return;

        bgmSource.clip = bgmClip;
        bgmSource.Play();
        isBgmPaused = false;
    }

    public void PauseBgm()
    {
        if (bgmSource == null || !bgmSource.isPlaying) return;

        bgmSource.Pause();
        isBgmPaused = true;
    }

    public void ResumeBgm()
    {
        if (bgmSource == null) return;

        if (isBgmPaused)
        {
            bgmSource.UnPause();
            isBgmPaused = false;
        }
        else
        {
            PlayBgm();
        }
    }

    public void StopBgm()
    {
        if (bgmSource == null) return;

        if (bgmFadeRoutine != null)
        {
            StopCoroutine(bgmFadeRoutine);
            bgmFadeRoutine = null;
        }

        bgmSource.Stop();
        isBgmPaused = false;
    }

    public void CrossfadeBGM(AudioClip newClip, float duration = 0.6f)
    {
        if (bgmSource == null) return;

        if (bgmFadeRoutine != null)
            StopCoroutine(bgmFadeRoutine);

        bgmFadeRoutine = StartCoroutine(CrossfadeRoutine(newClip, Mathf.Max(0.01f, duration)));
    }

    IEnumerator CrossfadeRoutine(AudioClip newClip, float duration)
    {
        float t = 0f;
        float startVol = bgmSource.volume;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            bgmSource.volume = Mathf.Lerp(startVol, 0f, t / duration);
            yield return null;
        }

        bgmSource.Stop();

        if (newClip != null)
        {
            bgmClip = newClip;
            bgmSource.clip = bgmClip;
            bgmSource.Play();

            t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                bgmSource.volume = Mathf.Lerp(0f, 1f, t / duration);
                yield return null;
            }
        }

        bgmFadeRoutine = null;
    }

    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;

        if (sfxSource != null) sfxSource.mute = !enabled;
        if (bgmSource != null) bgmSource.mute = !enabled;

        PlayerPrefs.SetInt("Sound", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetMuted(bool muted)
    {
        SetSoundEnabled(!muted);
    }

    public void SetMutedFromToggle(bool isOn)
    {
        SetSoundEnabled(isOn);
    }

    public void PlayUI() => PlayOneShot(uiClip, false);
    public void PlayLevelUp() => PlayOneShot(levelUpClip, false);
    public void PlayGameOverSfx() => PlayOneShot(gameOverClip, false);

    // Keep this for existing GameManager call
    public void PlayPop() => PlayOneShot(foodClip, true);

    public void PlayFood(FoodType foodType)
    {
        switch (foodType)
        {
            case FoodType.SpeedBoost:
                PlayOneShot(speedBoostClip != null ? speedBoostClip : foodClip, true);
                break;
            case FoodType.Slow:
                PlayOneShot(slowClip != null ? slowClip : foodClip, true);
                break;
            default:
                PlayOneShot(foodClip, true);
                break;
        }
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, bool randomPitch = true)
    {
        if (!isSoundEnabled || sfxSource == null || clip == null) return;

        float originalPitch = sfxSource.pitch;
        if (randomPitch)
            sfxSource.pitch = Random.Range(sfxPitchMin, sfxPitchMax);

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        sfxSource.pitch = originalPitch;
    }

    void PlayOneShot(AudioClip clip, bool randomPitch)
    {
        if (!isSoundEnabled || sfxSource == null || clip == null) return;

        float originalPitch = sfxSource.pitch;
        if (randomPitch)
            sfxSource.pitch = Random.Range(sfxPitchMin, sfxPitchMax);

        sfxSource.PlayOneShot(clip);
        sfxSource.pitch = originalPitch;
    }
}