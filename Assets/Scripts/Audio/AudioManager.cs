using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// Lightweight central audio router. Persists across scenes, owns one music
/// source and a small pool of one-shot SFX sources. Call from anywhere via
/// AudioManager.Instance — auto-instantiated on first access if absent.
[DefaultExecutionOrder(-200)]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer Routing (optional)")]
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup sfxGroup;

    [Header("Volumes (0..1)")]
    [Range(0f, 1f)] public float musicVolume = 0.55f;
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Header("SFX Pool")]
    [Min(2)] public int sfxPoolSize = 12;

    private AudioSource musicSource;
    private readonly List<AudioSource> sfxPool = new();
    private int sfxCursor;

    public static AudioManager EnsureExists()
    {
        if (Instance != null) return Instance;
        var go = new GameObject("AudioManager");
        return go.AddComponent<AudioManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.outputAudioMixerGroup = musicGroup;
        musicSource.volume = musicVolume;

        for (int i = 0; i < sfxPoolSize; i++)
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.outputAudioMixerGroup = sfxGroup;
            sfxPool.Add(src);
        }
    }

    /// Play looping music. Skips if the clip is already active.
    public void PlayMusic(AudioClip clip, bool fade = true)
    {
        if (clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null) musicSource.Stop();
    }

    /// 2D one-shot — for UI, player feedback, anything non-positional.
    public void PlaySfx2D(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        var src = NextSfxSource();
        src.spatialBlend = 0f;
        src.pitch = pitch;
        src.volume = sfxVolume * volumeScale;
        src.PlayOneShot(clip);
    }

    /// 3D positional one-shot at a world location.
    public void PlaySfxAt(AudioClip clip, Vector3 worldPos, float volumeScale = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        var src = NextSfxSource();
        src.transform.position = worldPos;
        src.spatialBlend = 1f;
        src.minDistance = 1.5f;
        src.maxDistance = 35f;
        src.rolloffMode = AudioRolloffMode.Linear;
        src.pitch = pitch;
        src.volume = sfxVolume * volumeScale;
        src.PlayOneShot(clip);
    }

    private AudioSource NextSfxSource()
    {
        if (sfxPool.Count == 0) return null;
        for (int i = 0; i < sfxPool.Count; i++)
        {
            sfxCursor = (sfxCursor + 1) % sfxPool.Count;
            if (!sfxPool[sfxCursor].isPlaying) return sfxPool[sfxCursor];
        }
        // All busy — reuse the next one anyway.
        sfxCursor = (sfxCursor + 1) % sfxPool.Count;
        return sfxPool[sfxCursor];
    }
}
