using UnityEngine;

/// Drop-in component: starts the assigned music track on Awake via AudioManager.
/// Convenient for gameplay scenes — attach to any GameObject in the scene root
/// and pick a clip in the inspector.
public class SceneMusic : MonoBehaviour
{
    [Tooltip("Looping music track that should play while this scene is active.")]
    public AudioClip track;

    [Tooltip("If true, stop any music when this scene is unloaded.")]
    public bool stopOnDestroy = false;

    private void Start()
    {
        if (track == null) return;
        AudioManager.EnsureExists().PlayMusic(track);
    }

    private void OnDestroy()
    {
        if (stopOnDestroy && AudioManager.Instance != null)
            AudioManager.Instance.StopMusic();
    }
}
