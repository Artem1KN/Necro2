using UnityEngine;

/// 3D billboard HP bar attached as a child Cube to an enemy.
/// The Cube's local X scale represents (currentHP / maxHP) and the Cube's
/// rotation faces the main camera each frame.
public class EnemySimpleHealthBar : MonoBehaviour
{
    [Header("Refs")]
    public EnemyBase target;
    public Transform fillTransform;
    public Renderer fillRenderer;

    [Header("Tuning")]
    public Vector3 fullScale = new(1.2f, 0.18f, 0.08f);
    public Color colorFull = new(0.2f, 1f, 0.3f, 1f);
    public Color colorLow = new(1f, 0.25f, 0.15f, 1f);

    private void Awake()
    {
        if (target == null) target = GetComponentInParent<EnemyBase>();
        if (fillTransform == null) fillTransform = transform;
        if (fillRenderer == null) fillRenderer = GetComponent<Renderer>();

        if (target != null) target.onHealthChanged += HandleHealthChanged;
    }

    private void OnDestroy()
    {
        if (target != null) target.onHealthChanged -= HandleHealthChanged;
    }

    private void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }

    private void HandleHealthChanged(float current, float max)
    {
        float pct = max > 0f ? Mathf.Clamp01(current / max) : 0f;
        if (fillTransform != null)
        {
            var s = fullScale;
            s.x *= pct;
            fillTransform.localScale = s;
        }

        if (fillRenderer != null && fillRenderer.material != null)
            fillRenderer.material.color = Color.Lerp(colorLow, colorFull, pct);
    }
}
