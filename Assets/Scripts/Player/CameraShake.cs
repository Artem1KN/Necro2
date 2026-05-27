using UnityEngine;

/// Camera shake driven by PlayerHealth damage events.
/// Attach to the camera holder (the transform that already has a Camera or
/// CinemachineCamera as a child). Applies a damped noise offset on the local
/// position so it composes safely with Cinemachine output.
public class CameraShake : MonoBehaviour
{
    [Header("Source")]
    [Tooltip("Optional explicit PlayerHealth ref. Auto-found if left null.")]
    public PlayerHealth playerHealth;

    [Header("Tuning")]
    [Tooltip("Position amplitude per unit of damage.")]
    public float positionPerDamage = 0.01f;

    [Tooltip("Maximum position amplitude in meters regardless of damage.")]
    public float maxPositionAmplitude = 0.25f;

    [Tooltip("Rotation amplitude (deg) per unit of damage.")]
    public float rotationPerDamage = 0.4f;

    [Tooltip("Maximum rotation amplitude in degrees regardless of damage.")]
    public float maxRotationAmplitude = 6f;

    [Tooltip("Frequency of the noise (Hz).")]
    public float frequency = 24f;

    [Tooltip("How fast the trauma value decays (per second).")]
    public float traumaDecay = 1.8f;

    private float trauma;
    private Vector3 baseLocalPos;
    private Quaternion baseLocalRot;
    private float lastHp = float.NaN;
    private float seedX, seedY, seedZ, seedRX, seedRY, seedRZ;

    private void Awake()
    {
        baseLocalPos = transform.localPosition;
        baseLocalRot = transform.localRotation;
        seedX = Random.Range(0f, 1000f);
        seedY = Random.Range(0f, 1000f);
        seedZ = Random.Range(0f, 1000f);
        seedRX = Random.Range(0f, 1000f);
        seedRY = Random.Range(0f, 1000f);
        seedRZ = Random.Range(0f, 1000f);
    }

    private void OnEnable()
    {
        if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged += HandleHpChanged;
            lastHp = playerHealth.CurrentHP;
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.onHealthChanged -= HandleHpChanged;
    }

    private void HandleHpChanged(float current, float max)
    {
        if (!float.IsNaN(lastHp) && current < lastHp)
        {
            float damage = lastHp - current;
            AddTrauma(damage);
        }
        lastHp = current;
    }

    /// Public entrypoint — other systems (parry, explosion proximity) can also push trauma.
    public void AddTrauma(float damage)
    {
        trauma = Mathf.Clamp01(trauma + damage * 0.05f);
        StartCoroutine_ShakeOnce(damage);
    }

    private void StartCoroutine_ShakeOnce(float damage) { /* trauma-driven via Update — kept for API parity */ }

    private void LateUpdate()
    {
        if (trauma <= 0f)
        {
            transform.localPosition = baseLocalPos;
            transform.localRotation = baseLocalRot;
            return;
        }

        float t = Time.unscaledTime * frequency;
        float shake = trauma * trauma; // quadratic curve feels more impactful

        float px = (Mathf.PerlinNoise(seedX, t) - 0.5f) * 2f * Mathf.Min(maxPositionAmplitude, positionPerDamage * 100f) * shake;
        float py = (Mathf.PerlinNoise(seedY, t) - 0.5f) * 2f * Mathf.Min(maxPositionAmplitude, positionPerDamage * 100f) * shake;
        float pz = (Mathf.PerlinNoise(seedZ, t) - 0.5f) * 2f * Mathf.Min(maxPositionAmplitude, positionPerDamage * 100f) * shake;

        float rx = (Mathf.PerlinNoise(seedRX, t) - 0.5f) * 2f * Mathf.Min(maxRotationAmplitude, rotationPerDamage * 100f) * shake;
        float ry = (Mathf.PerlinNoise(seedRY, t) - 0.5f) * 2f * Mathf.Min(maxRotationAmplitude, rotationPerDamage * 100f) * shake;
        float rz = (Mathf.PerlinNoise(seedRZ, t) - 0.5f) * 2f * Mathf.Min(maxRotationAmplitude, rotationPerDamage * 100f) * shake;

        transform.localPosition = baseLocalPos + new Vector3(px, py, pz);
        transform.localRotation = baseLocalRot * Quaternion.Euler(rx, ry, rz);

        trauma = Mathf.Max(0f, trauma - traumaDecay * Time.deltaTime);
    }
}
