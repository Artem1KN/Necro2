using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Главный контроллер HUD. Один компонент на корневом Canvas.
/// Слушает PlayerHealth.onHealthChanged, опрашивает WeaponManager.AllWeapons каждый кадр.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Refs — Player")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private PlayerMotor playerMotor;

    [Header("Refs — Vitality")]
    [SerializeField] private Image hpFill;
    [SerializeField] private TMP_Text hpText;

    [Header("Refs — Arsenal")]
    [SerializeField] private Transform arsenalParent;
    [SerializeField] private WeaponSlotUI slotPrefab;
    [Tooltip("Иконки оружий, по индексам слотов WeaponManager.AllWeapons. Длина должна совпадать.")]
    [SerializeField] private Sprite[] weaponIcons;

    [Header("Refs — Thermal")]
    [SerializeField] private Image mainHeatFill;
    [SerializeField] private TMP_Text dmgBonusText;
    [SerializeField] private Image speedGaugeFill;
    [SerializeField] private TMP_Text speedText;

    [Header("Colors")]
    [SerializeField] private Color dmgBonusOnColor = new Color(0f, 1f, 1f);
    [SerializeField] private Color dmgBonusOffColor = new Color(0.33f, 0.33f, 0.33f);

    [Header("Speedometer")]
    [Tooltip("Скорость, при которой gauge заполняется полностью.")]
    [SerializeField] private float maxSpeedForGauge = 15f;

    private readonly List<WeaponSlotUI> spawnedSlots = new List<WeaponSlotUI>();

    private void Start()
    {
        if (playerHealth != null)
        {
            playerHealth.onHealthChanged += HandleHealthChanged;
            HandleHealthChanged(playerHealth.CurrentHP, playerHealth.MaxHP);
        }

        BuildArsenal();
    }

    private void OnDestroy()
    {
        if (playerHealth != null) playerHealth.onHealthChanged -= HandleHealthChanged;
    }

    private void Update()
    {
        UpdateArsenal();
        UpdateThermal();
        UpdateSpeed();
    }

    private void HandleHealthChanged(float current, float max)
    {
        float pct = max > 0f ? current / max : 0f;
        if (hpFill != null) hpFill.fillAmount = pct;
        if (hpText != null) hpText.text = $"HP: {Mathf.CeilToInt(current):D3}/{Mathf.CeilToInt(max):D3}";
    }

    private void BuildArsenal()
    {
        if (weaponManager == null || slotPrefab == null || arsenalParent == null) return;

        var weapons = weaponManager.AllWeapons;
        if (weapons == null) return;

        for (int i = 0; i < weapons.Count; i++)
        {
            var slot = Instantiate(slotPrefab, arsenalParent);
            spawnedSlots.Add(slot);
        }
    }

    private void UpdateArsenal()
    {
        if (weaponManager == null) return;
        var weapons = weaponManager.AllWeapons;
        if (weapons == null) return;

        int activeIdx = weaponManager.CurrentSlot;
        for (int i = 0; i < spawnedSlots.Count && i < weapons.Count; i++)
        {
            var weapon = weapons[i];
            if (weapon == null) continue;

            float heatPct = weapon.data != null && weapon.data.overheatThreshold > 0f
                ? weapon.currentHeat / weapon.data.overheatThreshold
                : 0f;

            Sprite icon = (weaponIcons != null && i < weaponIcons.Length) ? weaponIcons[i] : null;
            bool unlocked = weapon.data != null && weapon.data.isAchieved;

            spawnedSlots[i].Bind(i + 1, icon, heatPct, i == activeIdx, unlocked);
        }
    }

    private void UpdateThermal()
    {
        if (weaponManager == null) return;
        var active = weaponManager.ActiveWeapon;
        if (active == null || active.data == null) return;

        float threshold = active.data.overheatThreshold > 0f ? active.data.overheatThreshold : 1f;
        float heatPct = Mathf.Clamp01(active.currentHeat / threshold);
        if (mainHeatFill != null) mainHeatFill.fillAmount = heatPct;

        float heat = active.currentHeat;
        bool inOptimal = heat >= active.data.optimalZoneStart && heat <= active.data.optimalZoneEnd;
        if (dmgBonusText != null)
        {
            dmgBonusText.text = inOptimal ? "DMG BONUS: ON" : "DMG BONUS: OFF";
            dmgBonusText.color = inOptimal ? dmgBonusOnColor : dmgBonusOffColor;
        }
    }

    private void UpdateSpeed()
    {
        if (playerMotor == null) return;

        float pct = maxSpeedForGauge > 0f ? Mathf.Clamp01(playerMotor.currentSpeed / maxSpeedForGauge) : 0f;
        if (speedGaugeFill != null) speedGaugeFill.fillAmount = pct;
        if (speedText != null) speedText.text = $"{Mathf.RoundToInt(playerMotor.currentSpeed)}";
    }
}
