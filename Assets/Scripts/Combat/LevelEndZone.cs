using UnityEngine;
using UnityEngine.SceneManagement;

/// Trigger volume placed at the end of a level. On player entry shows the
/// LevelCompleteScreen and unlocks the cursor; the screen handles Next Level.
[RequireComponent(typeof(Collider))]
public class LevelEndZone : MonoBehaviour
{
    [Header("Trigger")]
    public LayerMask playerLayer = ~0;

    [Header("Screen")]
    [Tooltip("Reference to a LevelCompleteScreen present in the scene (initially disabled).")]
    public LevelCompleteScreen completeScreen;

    [Header("Behavior")]
    [Tooltip("Pause the game (Time.timeScale = 0) while the screen is visible.")]
    public bool pauseOnComplete = true;

    private bool triggered;

    private void Awake()
    {
        var col = GetComponent<Collider>();
        if (!col.isTrigger) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (((1 << other.gameObject.layer) & playerLayer.value) == 0) return;

        triggered = true;

        if (pauseOnComplete) Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (completeScreen != null) completeScreen.Show();
    }
}
