using UnityEngine;

[CreateAssetMenu(fileName = "NewRoomCombat", menuName = "Necro2/Combat/Room Combat Data")]
public class RoomCombatData : ScriptableObject
{
    [Header("Identity")]
    public string roomName = "Arena";

    [Header("Waves")]
    public WaveData[] waves;

    [Header("Behavior")]
    [Tooltip("Toggle GameObjects in RoomCombat.doorsToClose during combat.")]
    public bool lockDoorsDuringCombat = true;

    [Tooltip("Disable the trigger after clear so the arena cannot restart.")]
    public bool oneShot = true;
}
