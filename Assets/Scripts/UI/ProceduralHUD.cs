using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Builds the full HUD programmatically at runtime following the layout from
/// Assets/Custom Assets/hud maket.html. Attach to any GameObject in the gameplay
/// scene and assign Player references. No scene-side Canvas needed — this script
/// owns the entire UI tree it creates in Awake.
///
/// Layout:
///   Bottom-Left  : HP gradient bar + "HP: nnn/mmm".
///   Bottom-Center: row of slots, one per WeaponManager.AllWeapons entry. Each
///                  slot shows the slot index, a heat mini-bar and a danger
///                  flag when heat is high. Active slot is highlighted.
///   Bottom-Right : speedometer + weapon heat bar with DMG BONUS zone + label.
///   Top-Center   : "WAVE n / N" + "ENEMIES: k" (only while RoomCombat active).
public class ProceduralHUD : MonoBehaviour
{
    [Header("Refs")]
    public PlayerHealth playerHealth;
    public WeaponManager weaponManager;
    public PlayerMotor playerMotor;

    [Header("Tuning")]
    public float maxSpeedForGauge = 15f;
    [Range(0f, 100f)] public float dangerHeatPercent = 80f;

    [Header("Crosshair")]
    public bool showCrosshair = true;
    public float crosshairSize = 18f;
    public float crosshairThickness = 2f;
    public Color crosshairColor = new(1f, 1f, 1f, 0.85f);
    public Color crosshairOverheatedColor = new(1f, 0.3f, 0.3f, 0.95f);
    public float crosshairGapPx = 4f;

    [Header("Colors")]
    public Color heatNormalColor = new(0.22f, 1f, 0.08f);
    public Color heatDangerColor = new(1f, 0.19f, 0.19f);
    public Color hpHighColor = new(0.22f, 1f, 0.08f);
    public Color hpLowColor = new(1f, 0.19f, 0.19f);
    public Color bonusOnColor = new(0f, 1f, 1f);
    public Color bonusOffColor = new(0.4f, 0.4f, 0.4f);

    private Canvas canvas;
    private Image hpFill;
    private TMP_Text hpText;

    private RectTransform hotbarRoot;
    private readonly List<SlotUI> slots = new();

    private Image heatFill;
    private Image heatBonusZone;
    private TMP_Text bonusText;
    private Image speedFill;
    private TMP_Text speedText;

    private RectTransform waveRoot;
    private TMP_Text waveText;
    private TMP_Text enemiesText;
    private CanvasGroup waveGroup;

    private RoomCombat trackedRoom;

    private Image crosshairTop;
    private Image crosshairBottom;
    private Image crosshairLeft;
    private Image crosshairRight;

    private struct SlotUI
    {
        public GameObject root;
        public Image background;
        public Image heatBar;
        public TMP_Text indexLabel;
        public GameObject dangerFlag;
        public GameObject lockOverlay;
    }

    private void OnEnable()
    {
        CombatManager.RoomActivated += OnRoomActivated;
        CombatManager.RoomCleared += OnRoomCleared;
    }

    private void OnDisable()
    {
        CombatManager.RoomActivated -= OnRoomActivated;
        CombatManager.RoomCleared -= OnRoomCleared;
        if (playerHealth != null) playerHealth.onHealthChanged -= HandleHealthChanged;
    }

    private void Start()
    {
        AutoBindRefs();

        BuildRoot();
        BuildHpBar();
        BuildHotbar();
        BuildThermal();
        BuildWaveCounter();
        if (showCrosshair) BuildCrosshair();

        if (playerHealth != null)
        {
            playerHealth.onHealthChanged += HandleHealthChanged;
            HandleHealthChanged(playerHealth.CurrentHP, playerHealth.MaxHP);
        }
    }

    private void AutoBindRefs()
    {
        if (playerHealth == null) playerHealth = FindObjectOfType<PlayerHealth>();
        if (weaponManager == null) weaponManager = FindObjectOfType<WeaponManager>();
        if (playerMotor == null) playerMotor = FindObjectOfType<PlayerMotor>();
    }

    private void Update()
    {
        UpdateHotbar();
        UpdateThermal();
        UpdateSpeed();
        UpdateWave();
        UpdateCrosshair();
    }

    private void BuildCrosshair()
    {
        var root = CreateRect("Crosshair", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        root.sizeDelta = new Vector2(crosshairSize * 2 + crosshairGapPx * 2, crosshairSize * 2 + crosshairGapPx * 2);
        crosshairTop = MakeCrosshairLine(root, new Vector2(0, crosshairGapPx + crosshairSize * 0.5f), new Vector2(crosshairThickness, crosshairSize));
        crosshairBottom = MakeCrosshairLine(root, new Vector2(0, -(crosshairGapPx + crosshairSize * 0.5f)), new Vector2(crosshairThickness, crosshairSize));
        crosshairLeft = MakeCrosshairLine(root, new Vector2(-(crosshairGapPx + crosshairSize * 0.5f), 0), new Vector2(crosshairSize, crosshairThickness));
        crosshairRight = MakeCrosshairLine(root, new Vector2(crosshairGapPx + crosshairSize * 0.5f, 0), new Vector2(crosshairSize, crosshairThickness));
    }

    private Image MakeCrosshairLine(RectTransform parent, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject("Line", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var img = go.GetComponent<Image>();
        img.color = crosshairColor;
        img.raycastTarget = false;
        return img;
    }

    private void UpdateCrosshair()
    {
        if (crosshairTop == null) return;
        Color c = crosshairColor;
        if (weaponManager != null)
        {
            var w = weaponManager.ActiveWeapon;
            if (w != null && w.isOverheated) c = crosshairOverheatedColor;
        }
        crosshairTop.color = c;
        crosshairBottom.color = c;
        crosshairLeft.color = c;
        crosshairRight.color = c;
    }

    private void BuildRoot()
    {
        var go = new GameObject("HUD_Procedural", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        go.transform.SetParent(transform, false);
        canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5;

        var scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private void BuildHpBar()
    {
        var container = CreateRect("HPBar", canvas.transform, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0));
        container.anchoredPosition = new Vector2(30, 40);
        container.sizeDelta = new Vector2(280, 40);

        var bg = CreateImage(container, new Color(0.05f, 0.05f, 0.05f, 0.85f));
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.sizeDelta = Vector2.zero;
        bg.raycastTarget = false;

        var fillRt = CreateRect("Fill", container.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0.5f));
        fillRt.offsetMin = new Vector2(4, 4);
        fillRt.offsetMax = new Vector2(-4, -4);
        hpFill = fillRt.gameObject.AddComponent<Image>();
        hpFill.color = hpHighColor;
        hpFill.type = Image.Type.Filled;
        hpFill.fillMethod = Image.FillMethod.Horizontal;
        hpFill.fillAmount = 1f;
        hpFill.raycastTarget = false;

        var textRt = CreateRect("Text", container.transform, new Vector2(0, 0), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        textRt.sizeDelta = Vector2.zero;
        hpText = textRt.gameObject.AddComponent<TextMeshProUGUI>();
        hpText.text = "HP 100/100";
        hpText.fontSize = 18;
        hpText.fontStyle = FontStyles.Bold;
        hpText.alignment = TextAlignmentOptions.Center;
        hpText.color = Color.white;
        hpText.raycastTarget = false;
    }

    private void BuildHotbar()
    {
        hotbarRoot = CreateRect("Hotbar", canvas.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0));
        hotbarRoot.anchoredPosition = new Vector2(0, 40);
        hotbarRoot.sizeDelta = new Vector2(560, 64);

        var bg = CreateImage(hotbarRoot, new Color(0.04f, 0.05f, 0.04f, 0.85f));
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.sizeDelta = Vector2.zero;
        bg.raycastTarget = false;

        var layout = hotbarRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(8, 8, 6, 6);
        layout.spacing = 6;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
    }

    private SlotUI BuildSlot(int index)
    {
        var slot = new GameObject($"Slot_{index + 1}", typeof(RectTransform), typeof(Image));
        slot.transform.SetParent(hotbarRoot, false);
        var img = slot.GetComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.14f, 1f);
        img.raycastTarget = false;

        var indexGo = new GameObject("Index", typeof(RectTransform), typeof(TextMeshProUGUI));
        indexGo.transform.SetParent(slot.transform, false);
        var indexRt = indexGo.GetComponent<RectTransform>();
        indexRt.anchorMin = new Vector2(0, 1);
        indexRt.anchorMax = new Vector2(0, 1);
        indexRt.pivot = new Vector2(0, 1);
        indexRt.anchoredPosition = new Vector2(4, -2);
        indexRt.sizeDelta = new Vector2(20, 16);
        var indexLabel = indexGo.GetComponent<TextMeshProUGUI>();
        indexLabel.text = (index + 1).ToString();
        indexLabel.fontSize = 12;
        indexLabel.color = new Color(0.8f, 0.8f, 0.8f);
        indexLabel.raycastTarget = false;

        var heatGo = new GameObject("HeatMini", typeof(RectTransform), typeof(Image));
        heatGo.transform.SetParent(slot.transform, false);
        var heatRt = heatGo.GetComponent<RectTransform>();
        heatRt.anchorMin = new Vector2(0, 0);
        heatRt.anchorMax = new Vector2(1, 0);
        heatRt.pivot = new Vector2(0.5f, 0);
        heatRt.anchoredPosition = new Vector2(0, 3);
        heatRt.sizeDelta = new Vector2(-8, 4);
        var heatBar = heatGo.GetComponent<Image>();
        heatBar.color = heatNormalColor;
        heatBar.type = Image.Type.Filled;
        heatBar.fillMethod = Image.FillMethod.Horizontal;
        heatBar.fillAmount = 0f;
        heatBar.raycastTarget = false;

        var dangerGo = new GameObject("Danger", typeof(RectTransform), typeof(Image));
        dangerGo.transform.SetParent(slot.transform, false);
        var dangerRt = dangerGo.GetComponent<RectTransform>();
        dangerRt.anchorMin = new Vector2(1, 1);
        dangerRt.anchorMax = new Vector2(1, 1);
        dangerRt.pivot = new Vector2(1, 1);
        dangerRt.anchoredPosition = new Vector2(-3, -3);
        dangerRt.sizeDelta = new Vector2(10, 10);
        var dangerImg = dangerGo.GetComponent<Image>();
        dangerImg.color = new Color(1, 0.19f, 0.19f);
        dangerImg.raycastTarget = false;
        dangerGo.SetActive(false);

        var lockGo = new GameObject("Lock", typeof(RectTransform), typeof(Image));
        lockGo.transform.SetParent(slot.transform, false);
        var lockRt = lockGo.GetComponent<RectTransform>();
        lockRt.anchorMin = Vector2.zero;
        lockRt.anchorMax = Vector2.one;
        lockRt.sizeDelta = Vector2.zero;
        var lockImg = lockGo.GetComponent<Image>();
        lockImg.color = new Color(0, 0, 0, 0.55f);
        lockImg.raycastTarget = false;
        lockGo.SetActive(false);

        return new SlotUI
        {
            root = slot,
            background = img,
            heatBar = heatBar,
            indexLabel = indexLabel,
            dangerFlag = dangerGo,
            lockOverlay = lockGo
        };
    }

    private void BuildThermal()
    {
        var container = CreateRect("Thermal", canvas.transform, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0));
        container.anchoredPosition = new Vector2(-30, 40);
        container.sizeDelta = new Vector2(300, 80);

        var bg = CreateImage(container, new Color(0.04f, 0.05f, 0.04f, 0.85f));
        bg.rectTransform.anchorMin = Vector2.zero;
        bg.rectTransform.anchorMax = Vector2.one;
        bg.rectTransform.sizeDelta = Vector2.zero;
        bg.raycastTarget = false;

        var speedRt = CreateRect("Speed", container.transform, new Vector2(0, 0.5f), new Vector2(0, 1), new Vector2(0, 0.5f));
        speedRt.anchoredPosition = new Vector2(40, 0);
        speedRt.sizeDelta = new Vector2(36, 36);
        var sBg = speedRt.gameObject.AddComponent<Image>();
        sBg.color = new Color(0.1f, 0.1f, 0.12f);
        sBg.raycastTarget = false;
        var sf = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        sf.transform.SetParent(speedRt, false);
        var sfRt = sf.GetComponent<RectTransform>();
        sfRt.anchorMin = Vector2.zero;
        sfRt.anchorMax = Vector2.one;
        sfRt.sizeDelta = Vector2.zero;
        speedFill = sf.GetComponent<Image>();
        speedFill.color = new Color(0, 1f, 1f);
        speedFill.type = Image.Type.Filled;
        speedFill.fillMethod = Image.FillMethod.Radial360;
        speedFill.fillOrigin = (int)Image.Origin360.Top;
        speedFill.fillAmount = 0f;
        speedFill.raycastTarget = false;
        var sLabel = CreateRect("Label", container.transform, new Vector2(0, 0.5f), new Vector2(0, 1), new Vector2(0, 0.5f));
        sLabel.anchoredPosition = new Vector2(150, 0);
        sLabel.sizeDelta = new Vector2(180, 30);
        speedText = sLabel.gameObject.AddComponent<TextMeshProUGUI>();
        speedText.text = "COOLING VELOCITY";
        speedText.fontSize = 12;
        speedText.color = new Color(0.7f, 0.7f, 0.7f);
        speedText.raycastTarget = false;

        var heatRt = CreateRect("Heat", container.transform, new Vector2(0, 0), new Vector2(1, 0.5f), new Vector2(0.5f, 0));
        heatRt.anchoredPosition = new Vector2(0, 8);
        heatRt.sizeDelta = new Vector2(-20, 18);
        var hBg = heatRt.gameObject.AddComponent<Image>();
        hBg.color = new Color(0.1f, 0.1f, 0.12f);
        hBg.raycastTarget = false;
        var bonusZone = new GameObject("BonusZone", typeof(RectTransform), typeof(Image));
        bonusZone.transform.SetParent(heatRt, false);
        var bzRt = bonusZone.GetComponent<RectTransform>();
        bzRt.anchorMin = new Vector2(0.7f, 0);
        bzRt.anchorMax = new Vector2(1, 1);
        bzRt.sizeDelta = Vector2.zero;
        heatBonusZone = bonusZone.GetComponent<Image>();
        heatBonusZone.color = new Color(1f, 0.68f, 0f, 0.35f);
        heatBonusZone.raycastTarget = false;
        var hFillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        hFillGo.transform.SetParent(heatRt, false);
        var hFillRt = hFillGo.GetComponent<RectTransform>();
        hFillRt.anchorMin = Vector2.zero;
        hFillRt.anchorMax = Vector2.one;
        hFillRt.sizeDelta = Vector2.zero;
        heatFill = hFillGo.GetComponent<Image>();
        heatFill.color = heatNormalColor;
        heatFill.type = Image.Type.Filled;
        heatFill.fillMethod = Image.FillMethod.Horizontal;
        heatFill.fillAmount = 0f;
        heatFill.raycastTarget = false;

        var bonusRt = CreateRect("Bonus", container.transform, new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 0));
        bonusRt.anchoredPosition = new Vector2(-10, 2);
        bonusRt.sizeDelta = new Vector2(280, 14);
        bonusText = bonusRt.gameObject.AddComponent<TextMeshProUGUI>();
        bonusText.text = "DMG BONUS: OFF";
        bonusText.fontSize = 11;
        bonusText.alignment = TextAlignmentOptions.Right;
        bonusText.color = bonusOffColor;
        bonusText.raycastTarget = false;
    }

    private void BuildWaveCounter()
    {
        waveRoot = CreateRect("Wave", canvas.transform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1));
        waveRoot.anchoredPosition = new Vector2(0, -20);
        waveRoot.sizeDelta = new Vector2(280, 60);

        waveGroup = waveRoot.gameObject.AddComponent<CanvasGroup>();
        waveGroup.alpha = 0f;

        var bg = waveRoot.gameObject.AddComponent<Image>();
        bg.color = new Color(0, 0, 0, 0.55f);
        bg.raycastTarget = false;

        var wt = CreateRect("WaveText", waveRoot, new Vector2(0, 0.45f), new Vector2(1, 1), new Vector2(0.5f, 0.5f));
        wt.sizeDelta = Vector2.zero;
        waveText = wt.gameObject.AddComponent<TextMeshProUGUI>();
        waveText.text = "WAVE 1 / 1";
        waveText.fontSize = 22;
        waveText.fontStyle = FontStyles.Bold;
        waveText.alignment = TextAlignmentOptions.Center;
        waveText.color = new Color(1, 0.85f, 0.2f);
        waveText.raycastTarget = false;

        var et = CreateRect("EnemiesText", waveRoot, new Vector2(0, 0), new Vector2(1, 0.45f), new Vector2(0.5f, 0.5f));
        et.sizeDelta = Vector2.zero;
        enemiesText = et.gameObject.AddComponent<TextMeshProUGUI>();
        enemiesText.text = "ENEMIES 0";
        enemiesText.fontSize = 14;
        enemiesText.alignment = TextAlignmentOptions.Center;
        enemiesText.color = new Color(1, 0.45f, 0.45f);
        enemiesText.raycastTarget = false;
    }

    private void UpdateHotbar()
    {
        if (weaponManager == null) return;
        var weapons = weaponManager.allWeapons;
        if (weapons == null) return;

        while (slots.Count < weapons.Count) slots.Add(BuildSlot(slots.Count));

        int active = weaponManager.CurrentSlot;
        for (int i = 0; i < slots.Count; i++)
        {
            bool inRange = i < weapons.Count;
            slots[i].root.SetActive(inRange);
            if (!inRange) continue;

            var w = weapons[i];
            if (w == null) continue;
            float thr = w.data != null && w.data.overheatThreshold > 0 ? w.data.overheatThreshold : 1f;
            float pct = Mathf.Clamp01(w.currentHeat / thr);

            slots[i].heatBar.fillAmount = pct;
            bool danger = pct * 100f >= dangerHeatPercent;
            slots[i].heatBar.color = danger ? heatDangerColor : heatNormalColor;
            slots[i].dangerFlag.SetActive(danger);

            bool unlocked = w.data == null || w.data.isAchieved;
            slots[i].lockOverlay.SetActive(!unlocked);

            if (i == active)
                slots[i].background.color = new Color(0.05f, 0.4f, 0.5f, 1f);
            else
                slots[i].background.color = new Color(0.12f, 0.12f, 0.14f, 1f);
        }
    }

    private void UpdateThermal()
    {
        if (weaponManager == null) return;
        var active = weaponManager.ActiveWeapon;
        if (active == null || active.data == null)
        {
            if (heatFill != null) heatFill.fillAmount = 0f;
            if (bonusText != null) { bonusText.text = "DMG BONUS: OFF"; bonusText.color = bonusOffColor; }
            return;
        }

        float thr = active.data.overheatThreshold > 0 ? active.data.overheatThreshold : 1f;
        float pct = Mathf.Clamp01(active.currentHeat / thr);
        heatFill.fillAmount = pct;

        bool inZone = active.currentHeat >= active.data.optimalZoneStart
            && active.currentHeat <= active.data.optimalZoneEnd;
        bonusText.text = inZone ? "DMG BONUS: ON" : "DMG BONUS: OFF";
        bonusText.color = inZone ? bonusOnColor : bonusOffColor;
    }

    private void UpdateSpeed()
    {
        if (speedFill == null || playerMotor == null) return;
        float pct = maxSpeedForGauge > 0 ? Mathf.Clamp01(playerMotor.currentSpeed / maxSpeedForGauge) : 0f;
        speedFill.fillAmount = pct;
    }

    private void UpdateWave()
    {
        if (trackedRoom == null)
        {
            if (waveGroup != null) waveGroup.alpha = 0f;
            return;
        }

        if (waveGroup != null) waveGroup.alpha = 1f;
        if (waveText != null) waveText.text = $"WAVE {trackedRoom.CurrentWaveIndex + 1} / {trackedRoom.TotalWaves}";
        if (enemiesText != null) enemiesText.text = $"ENEMIES {trackedRoom.AliveEnemyCount}";
    }

    private void HandleHealthChanged(float current, float max)
    {
        float pct = max > 0 ? current / max : 0f;
        if (hpFill != null)
        {
            hpFill.fillAmount = pct;
            hpFill.color = Color.Lerp(hpLowColor, hpHighColor, pct);
        }
        if (hpText != null) hpText.text = $"HP {Mathf.CeilToInt(current):D3}/{Mathf.CeilToInt(max):D3}";
    }

    private void OnRoomActivated(RoomCombat room) => trackedRoom = room;
    private void OnRoomCleared(RoomCombat room) { if (trackedRoom == room) trackedRoom = null; }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        return rt;
    }

    private static Image CreateImage(RectTransform parent, Color color)
    {
        var go = new GameObject("Bg", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }
}
