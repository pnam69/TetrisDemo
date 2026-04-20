using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource sfxSource;
    public AudioSource bgmSource;

    // game clips
    public AudioClip flapClip;
    public AudioClip scoreClip;
    public AudioClip hitClip;
    public AudioClip gameOverClip;
    public AudioClip bgmClip;

    private bool isBgmPaused = false;
    private bool isSoundEnabled = true;

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
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

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

    // legacy API

    // game API
    public void PlayFlap() => PlayOneShot(flapClip);
    public void PlayScore() => PlayOneShot(scoreClip);
    public void PlayHit() => PlayOneShot(hitClip);
    public void PlayGameOver()
    {
        PlayOneShot(gameOverClip);
        StopBgm();
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
        if (bgmSource == null) return;
        if (bgmSource.isPlaying)
        {
            bgmSource.Pause();
            isBgmPaused = true;
        }
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
        bgmSource.Stop();
        isBgmPaused = false;
    }

    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;
        if (sfxSource != null) sfxSource.mute = !enabled;
        if (bgmSource != null) bgmSource.mute = !enabled;
        PlayerPrefs.SetInt("Sound", enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    void PlayOneShot(AudioClip clip)
    {
        if (!isSoundEnabled) return;
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
