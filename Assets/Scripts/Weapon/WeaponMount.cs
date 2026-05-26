using UnityEngine;

/// Attaches a weapon GameObject to the main camera at runtime with the
/// configured local offset, rotation and scale. This guarantees first-person
/// weapons follow camera pitch/yaw regardless of where they sit in the prefab
/// hierarchy (fixes "weapon stays behind player's head" bug).
[DefaultExecutionOrder(-100)]
public class WeaponMount : MonoBehaviour
{
    [Header("Camera Anchor")]
    [Tooltip("Local offset from the main camera. Positive X = right, negative Y = down, positive Z = forward.")]
    public Vector3 localOffset = new(0.3f, -0.25f, 0.6f);

    [Tooltip("Local euler angles relative to the main camera.")]
    public Vector3 localEuler = Vector3.zero;

    [Tooltip("Multiplier applied to local scale after parenting (1 keeps existing scale).")]
    public float localScaleMultiplier = 1f;

    [Tooltip("If true, also reparent under the camera. If false, only positions/rotates the existing transform.")]
    public bool reparentUnderCamera = true;

    private void Awake()
    {
        var cam = Camera.main;
        if (cam == null) return;

        if (reparentUnderCamera) transform.SetParent(cam.transform, false);

        transform.localPosition = localOffset;
        transform.localRotation = Quaternion.Euler(localEuler);
        if (Mathf.Abs(localScaleMultiplier - 1f) > 0.001f)
            transform.localScale *= localScaleMultiplier;
    }
}
