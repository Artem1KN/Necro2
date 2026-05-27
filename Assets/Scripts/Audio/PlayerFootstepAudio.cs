using UnityEngine;

/// Plays footstep one-shots at a rate proportional to PlayerMotor.currentSpeed
/// while the player is grounded. Attach to the Player root.
[RequireComponent(typeof(PlayerMotor))]
public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip[] footstepClips;
    public AudioClip jumpClip;
    public AudioClip landClip;

    [Header("Tuning")]
    [Tooltip("Distance in meters between footsteps at run speed.")]
    public float stepDistance = 2.2f;

    [Tooltip("Minimum horizontal speed required to play footsteps.")]
    public float minSpeed = 1.5f;

    [Range(0f, 1f)] public float volume = 0.6f;

    private PlayerMotor motor;
    private CharacterController cc;
    private float accumulatedDistance;
    private bool wasGroundedLastFrame;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        cc = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (motor == null) return;
        bool grounded = cc != null && cc.isGrounded;

        if (!wasGroundedLastFrame && grounded && landClip != null)
        {
            AudioManager.EnsureExists().PlaySfx2D(landClip, volume);
        }
        wasGroundedLastFrame = grounded;

        if (!grounded)
        {
            accumulatedDistance = 0f;
            return;
        }

        float speed = motor.currentSpeed;
        if (speed < minSpeed) { accumulatedDistance = 0f; return; }

        accumulatedDistance += speed * Time.deltaTime;
        if (accumulatedDistance >= stepDistance)
        {
            accumulatedDistance = 0f;
            PlayRandomFootstep();
        }
    }

    public void PlayJump()
    {
        if (jumpClip != null) AudioManager.EnsureExists().PlaySfx2D(jumpClip, volume);
    }

    private void PlayRandomFootstep()
    {
        if (footstepClips == null || footstepClips.Length == 0) return;
        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        AudioManager.EnsureExists().PlaySfx2D(clip, volume * Random.Range(0.9f, 1.1f));
    }
}
