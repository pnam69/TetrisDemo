using UnityEngine;

// Simple AudioManager singleton that plays common SFX and enforces per-sound cooldowns
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources & Clips")]
    public AudioSource sfxSource;
    public AudioClip popClip;
    public AudioClip dropClip;
    public AudioClip shootClip;
    public AudioClip clickClip;

    [Header("Cooldowns (seconds)")]
    [Tooltip("Minimum interval between pop sounds")] public float popCooldown = 0.05f;
    [Tooltip("Minimum interval between drop/fall sounds")] public float dropCooldown = 0.35f;

    private float lastPopTime = -100f;
    private float lastDropTime = -100f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optional: don't destroy on load if you want persistent audio manager
        // DontDestroyOnLoad(gameObject);

        if (sfxSource == null)
        {
            sfxSource = gameObject.GetComponent<AudioSource>();
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public void SetPopCooldown(float seconds)
    {
        popCooldown = Mathf.Max(0f, seconds);
    }

    public void SetDropCooldown(float seconds)
    {
        dropCooldown = Mathf.Max(0f, seconds);
    }

    public void PlayPop()
    {
        if (sfxSource == null || popClip == null) return;
        float now = Time.unscaledTime;
        if (now - lastPopTime < popCooldown) return;
        lastPopTime = now;
        sfxSource.PlayOneShot(popClip);
    }

    public void PlayDrop()
    {
        if (sfxSource == null || dropClip == null) return;
        float now = Time.unscaledTime;
        if (now - lastDropTime < dropCooldown) return;
        lastDropTime = now;
        sfxSource.PlayOneShot(dropClip);
    }

    public void PlayShoot()
    {
        if (sfxSource == null || shootClip == null) return;
        sfxSource.PlayOneShot(shootClip);
    }

    public void PlayClick()
    {
        if (sfxSource == null || clickClip == null) return;
        sfxSource.PlayOneShot(clickClip);
    }
}
