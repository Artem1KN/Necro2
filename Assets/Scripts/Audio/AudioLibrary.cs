using UnityEngine;

/// Single ScriptableObject that bundles every clip the game uses.
/// Designed for one place where the designer drops files; gameplay code reads
/// them via AudioCue.Play() helpers that resolve the entry by enum.
[CreateAssetMenu(menuName = "Necro2/Audio/Audio Library", fileName = "AudioLibrary")]
public class AudioLibrary : ScriptableObject
{
    public static AudioLibrary Instance { get; private set; }

    [Header("Music")]
    public AudioClip musicMainMenu;
    public AudioClip musicGameplay;
    public AudioClip musicCombat;
    public AudioClip musicGameOver;

    [Header("Player")]
    public AudioClip playerHurt;
    public AudioClip playerDeath;
    public AudioClip playerFootstep;
    public AudioClip playerJump;
    public AudioClip playerLand;

    [Header("Weapons - Fire")]
    public AudioClip swordSwing;
    public AudioClip swordParry;
    public AudioClip assaultRifleFire;
    public AudioClip crossbowFire;
    public AudioClip shotgunFire;
    public AudioClip sniperFire;
    public AudioClip rocketLauncherFire;
    public AudioClip grenadeLauncherFire;
    public AudioClip weaponOverheat;
    public AudioClip weaponSwitch;

    [Header("Enemies")]
    public AudioClip zombieHurt;
    public AudioClip zombieDeath;
    public AudioClip zombieAttack;
    public AudioClip soldierHurt;
    public AudioClip soldierDeath;
    public AudioClip soldierFire;

    [Header("World")]
    public AudioClip energyOrbPickup;
    public AudioClip explosion;
    public AudioClip waveStart;
    public AudioClip waveCleared;
    public AudioClip levelComplete;

    private void OnEnable()
    {
        Instance = this;
    }

    /// Convenience — null-safe player.
    public static void Play2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        AudioManager.EnsureExists().PlaySfx2D(clip, volume);
    }

    public static void PlayAt(AudioClip clip, Vector3 pos, float volume = 1f)
    {
        if (clip == null) return;
        AudioManager.EnsureExists().PlaySfxAt(clip, pos, volume);
    }
}
