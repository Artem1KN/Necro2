using UnityEngine;

/// Spawns a short-lived emissive sphere at a hit point. Use Instantiate from weapon code.
public class HitEffect : MonoBehaviour
{
    [Tooltip("Lifetime in seconds before destroy.")]
    public float lifeSeconds = 0.25f;

    [Tooltip("Scale curve over lifetime (1 → 0 looks like a pop).")]
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    private float age;
    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        age += Time.deltaTime;
        if (age >= lifeSeconds)
        {
            Destroy(gameObject);
            return;
        }

        float t = age / lifeSeconds;
        transform.localScale = baseScale * scaleCurve.Evaluate(t);
    }
}
