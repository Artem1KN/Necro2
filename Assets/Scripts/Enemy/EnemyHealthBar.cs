using UnityEngine;
using UnityEngine.UI;

/// Billboard health bar that floats above an enemy. Attach to a World-Space Canvas
/// child of the enemy root. Tracks EnemyBase.onHealthChanged.
[RequireComponent(typeof(Canvas))]
public class EnemyHealthBar : MonoBehaviour
{
    [Header("Refs")]
    public EnemyBase target;
    public Image fillImage;
    public Image backgroundImage;

    [Header("Behavior")]
    public Vector3 worldOffset = new(0f, 1.4f, 0f);
    public bool hideWhenFull = true;

    private Camera mainCam;
    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        mainCam = Camera.main;

        if (target == null) target = GetComponentInParent<EnemyBase>();

        if (target != null)
        {
            target.onHealthChanged += HandleHealthChanged;
            HandleHealthChanged(target.MaxHP, target.MaxHP);
        }
    }

    private void OnDestroy()
    {
        if (target != null) target.onHealthChanged -= HandleHealthChanged;
    }

    private void LateUpdate()
    {
        if (target == null) return;
        if (mainCam == null) mainCam = Camera.main;

        transform.position = target.transform.position + worldOffset;
        if (mainCam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - mainCam.transform.position);
    }

    private void HandleHealthChanged(float current, float max)
    {
        if (fillImage != null)
            fillImage.fillAmount = max > 0f ? current / max : 0f;

        if (hideWhenFull)
        {
            bool show = current < max && current > 0f;
            if (fillImage != null) fillImage.enabled = show;
            if (backgroundImage != null) backgroundImage.enabled = show;
        }
    }
}
