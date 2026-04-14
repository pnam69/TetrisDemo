using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioSource sfxSource;
    public AudioSource bgmSource;

    public AudioClip shootClip;
    public AudioClip popClip;
    public AudioClip dropClip;
    public AudioClip bgmClip;

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
        PlayBgm();
    }

    public void PlayShoot()
    {
        PlayOneShot(shootClip);
    }

    public void PlayPop()
    {
        PlayOneShot(popClip);
    }

    public void PlayDrop()
    {
        PlayOneShot(dropClip);
    }

    public void PlayBgm()
    {
        if (bgmSource == null || bgmClip == null) return;
        if (bgmSource.isPlaying) return;

        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    void PlayOneShot(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
