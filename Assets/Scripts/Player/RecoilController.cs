using UnityEngine;

public class RecoilController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform that receives the recoil rotation. Usually the Cinemachine camera holder.")]
    [SerializeField] private Transform recoilPivot;

    [Header("Tuning")]
    [Tooltip("How fast the camera snaps to the kick rotation.")]
    [SerializeField] private float snapSpeed = 30f;

    [Tooltip("How fast the camera returns to neutral.")]
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
        if (recoilPivot == null)
        {
            var cam = Camera.main;
            if (cam != null) recoilPivot = cam.transform;
            else recoilPivot = transform;
        }
    }

    public void AddKick(float strength, float duration)
    {
        if (strength <= 0f) return;

        float pitch = -Mathf.Abs(strength);
        float yaw = Random.Range(-horizontalJitter, horizontalJitter) * strength;

        targetRecoil += new Vector3(pitch, yaw, 0f);
        kickDecayDuration = Mathf.Max(0.01f, duration);
        kickDecayTimer = kickDecayDuration;
    }

    private void LateUpdate()
    {
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

        recoilPivot.localRotation = Quaternion.Euler(currentRecoil);
    }
}
