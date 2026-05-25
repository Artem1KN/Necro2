using TMPro;
using UnityEngine;

/// Shows current wave / total waves and alive enemy count from CombatManager.ActiveRoom.
public class WaveCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemiesText;
    [SerializeField] private CanvasGroup canvasGroup;

    private RoomCombat tracked;

    private void OnEnable()
    {
        CombatManager.RoomActivated += OnRoomActivated;
        CombatManager.RoomCleared += OnRoomCleared;
        SetVisible(false);
    }

    private void OnDisable()
    {
        CombatManager.RoomActivated -= OnRoomActivated;
        CombatManager.RoomCleared -= OnRoomCleared;
    }

    private void Update()
    {
        if (tracked == null) return;

        if (waveText != null)
            waveText.text = $"WAVE {tracked.CurrentWaveIndex + 1} / {tracked.TotalWaves}";

        if (enemiesText != null)
            enemiesText.text = $"ENEMIES: {tracked.AliveEnemyCount}";
    }

    private void OnRoomActivated(RoomCombat room)
    {
        tracked = room;
        SetVisible(true);
    }

    private void OnRoomCleared(RoomCombat room)
    {
        if (tracked == room)
        {
            tracked = null;
            SetVisible(false);
        }
    }

    private void SetVisible(bool v)
    {
        if (canvasGroup != null) canvasGroup.alpha = v ? 1f : 0f;
    }
}
