using Unity.Cinemachine;
using UnityEngine;

/// High-level recoil entry point. Tries two paths in order:
/// 1. If a CinemachineRecoilExtension is found on the player's CinemachineCamera,
///    forwards the kick to it — this is the only way recoil survives the
///    Cinemachine pipeline, which otherwise overwrites Transform rotation in
///    LateUpdate.
/// 2. Falls back to rotating recoilPivot.localRotation — works for non-Cinemachine
///    setups or when the extension hasn't been added yet.
public class RecoilController : MonoBehaviour
{
    [Header("Cinemachine (preferred)")]
    [Tooltip("Cinemachine extension to forward kicks to. Auto-found at runtime if left empty.")]
    public CinemachineRecoilExtension cinemachineExtension;

    [Header("Fallback Transform Mode")]
    [Tooltip("Transform that receives the recoil rotation when no Cinemachine extension is present.")]
    [SerializeField] private Transform recoilPivot;

    [Tooltip("How fast the camera snaps to the kick rotation (transform fallback only).")]
    [SerializeField] private float snapSpeed = 30f;

    [Tooltip("How fast the camera returns to neutral (transform fallback only).")]
    [SerializeField] private float returnSpeed = 8f;

    [Tooltip("Horizontal kick randomness (±yaw degrees) per unit of strength.")]
    [SerializeField] private float horizontalJitter = 0.5f;

    private Vector3 currentRecoil;
    private Vector3 targetRecoil;
    private float kickDecayTimer;
    private float kickDecayDuration;

    private void Reset()
    {
        recoilPivot = transform;
    }

    private void Awake()
    {
        if (cinemachineExtension == null)
            cinemachineExtension = FindObjectOfType<CinemachineRecoilExtension>(true);

        if (recoilPivot == null)
        {
            var cam = Camera.main;
            recoilPivot = cam != null ? cam.transform : transform;
        }
    }

    public void AddKick(float strength, float duration)
    {
        if (strength <= 0f) return;

        if (cinemachineExtension == null)
            cinemachineExtension = FindObjectOfType<CinemachineRecoilExtension>(true);

        if (cinemachineExtension != null)
        {
            cinemachineExtension.AddKick(strength, duration);
            return;
        }

        float pitch = -Mathf.Abs(strength);
        float yaw = Random.Range(-horizontalJitter, horizontalJitter) * strength;

        targetRecoil += new Vector3(pitch, yaw, 0f);
        kickDecayDuration = Mathf.Max(0.01f, duration);
        kickDecayTimer = kickDecayDuration;
    }

    private void LateUpdate()
    {
        // Transform fallback only — when Cinemachine extension handles recoil, this branch idles.
        if (cinemachineExtension != null) return;

        if (kickDecayTimer > 0f)
        {
            kickDecayTimer -= Time.deltaTime;
            currentRecoil = Vector3.Slerp(currentRecoil, targetRecoil, snapSpeed * Time.deltaTime);
        }
        else
        {
            targetRecoil = Vector3.Slerp(targetRecoil, Vector3.zero, returnSpeed * Time.deltaTime);
            currentRecoil = Vector3.Slerp(currentRecoil, Vector3.zero, returnSpeed * Time.deltaTime);
        }

        if (recoilPivot != null)
            recoilPivot.localRotation = Quaternion.Euler(currentRecoil);
    }
}
