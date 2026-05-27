using Unity.Cinemachine;
using UnityEngine;

/// Cinemachine 3 extension that adds a recoil rotation on top of the camera
/// after the rest of the pipeline finishes. Add as an extension on the
/// CinemachineCamera that follows the player (Inspector → "Add Extension"
/// dropdown on the CinemachineCamera component).
///
/// RecoilController.AddKick() automatically forwards to this extension when
/// it's present on the active virtual camera — no extra wiring needed.
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Recoil Extension")]
public class CinemachineRecoilExtension : CinemachineExtension
{
    [Header("Tuning")]
    [Tooltip("How fast the camera snaps to the kicked rotation.")]
    public float snapSpeed = 30f;

    [Tooltip("How fast the camera returns to neutral after the kick window ends.")]
    public float returnSpeed = 8f;

    [Tooltip("Horizontal kick randomness (±yaw degrees) per unit of strength.")]
    public float horizontalJitter = 0.5f;

    private Vector3 currentRecoil;
    private Vector3 targetRecoil;
    private float kickDecayTimer;
    private float kickDecayDuration;

    public void AddKick(float strength, float duration)
    {
        if (strength <= 0f) return;

        float pitch = -Mathf.Abs(strength);
        float yaw = Random.Range(-horizontalJitter, horizontalJitter) * strength;

        targetRecoil += new Vector3(pitch, yaw, 0f);
        kickDecayDuration = Mathf.Max(0.01f, duration);
        kickDecayTimer = kickDecayDuration;
    }

    public void ResetRecoil()
    {
        currentRecoil = Vector3.zero;
        targetRecoil = Vector3.zero;
        kickDecayTimer = 0f;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize) return;
        if (deltaTime < 0f) deltaTime = Time.deltaTime;

        if (kickDecayTimer > 0f)
        {
            kickDecayTimer -= deltaTime;
            currentRecoil = Vector3.Slerp(currentRecoil, targetRecoil, snapSpeed * deltaTime);
        }
        else
        {
            targetRecoil = Vector3.Slerp(targetRecoil, Vector3.zero, returnSpeed * deltaTime);
            currentRecoil = Vector3.Slerp(currentRecoil, Vector3.zero, returnSpeed * deltaTime);
        }

        Quaternion kick = Quaternion.Euler(currentRecoil);
        state.RawOrientation = state.RawOrientation * kick;
    }
}
