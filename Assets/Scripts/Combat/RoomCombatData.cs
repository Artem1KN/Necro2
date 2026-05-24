using UnityEngine;

/// <summary>
/// Конфигурация боевой комнаты-арены: список волн и общие параметры.
/// </summary>
[CreateAssetMenu(fileName = "NewRoomCombat", menuName = "Necro2/Combat/Room Combat Data")]
public class RoomCombatData : ScriptableObject
{
    [Header("Identity")]
    public string roomName = "Arena";

    [Header("Waves")]
    [Tooltip("Список волн в порядке появления.")]
    public WaveData[] waves;

    [Header("Behavior")]
    [Tooltip("Закрывать ли двери на время боя (RoomCombat.doorsToClose).")]
    public bool lockDoorsDuringCombat = true;

    [Tooltip("Срабатывает только один раз — комната не перезапускается после очистки.")]
    public bool oneShot = true;
}
