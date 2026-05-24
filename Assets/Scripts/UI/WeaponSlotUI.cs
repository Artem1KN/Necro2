using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Один слот оружия в hotbar. Заполняется HUDController на каждый кадр.
/// На префаб слота нужно повесить этот компонент и проставить ссылки в инспекторе.
/// </summary>
public class WeaponSlotUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TMP_Text indexText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image heatMiniFill;
    [SerializeField] private GameObject lockOverlay;
    [SerializeField] private GameObject dangerFlag;
    [SerializeField] private GameObject activeFrame;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Colors")]
    [SerializeField] private Color heatNormalColor = new Color(0.22f, 1f, 0.08f);
    [SerializeField] private Color heatDangerColor = new Color(1f, 0.19f, 0.19f);

    [Header("Tuning")]
    [Tooltip("Heat % at which the danger flag appears.")]
    [Range(0f, 100f)]
    [SerializeField] private float dangerHeatPercent = 80f;

    public void Bind(int displayIndex, Sprite icon, float heatPercent01, bool isActive, bool isUnlocked)
    {
        if (indexText != null) indexText.text = displayIndex.ToString();
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null && isUnlocked;
        }

        float clamped = Mathf.Clamp01(heatPercent01);
        if (heatMiniFill != null)
        {
            heatMiniFill.fillAmount = clamped;
            heatMiniFill.color = clamped * 100f >= dangerHeatPercent ? heatDangerColor : heatNormalColor;
        }

        if (lockOverlay != null) lockOverlay.SetActive(!isUnlocked);
        if (dangerFlag != null) dangerFlag.SetActive(isUnlocked && clamped * 100f >= dangerHeatPercent);
        if (activeFrame != null) activeFrame.SetActive(isActive && isUnlocked);

        if (canvasGroup != null) canvasGroup.alpha = isUnlocked ? 1f : 0.5f;
    }
}
