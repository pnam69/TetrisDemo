using UnityEngine;

public static class SoundEffects
{
    public static void SetPopCooldown(float seconds)
    {
        AudioManager.Instance?.SetPopCooldown(seconds);
    }

    public static void SetDropCooldown(float seconds)
    {
        AudioManager.Instance?.SetDropCooldown(seconds);
    }

    public static void PlayPop()
    {
        AudioManager.Instance?.PlayPop();
    }

    public static void PlayDrop()
    {
        AudioManager.Instance?.PlayDrop();
    }
}
