using UnityEngine;

[CreateAssetMenu(fileName = "OrbData", menuName = "Gameplay/Orb Data")]
public class OrbData : ScriptableObject
{
    [Header("Heal Settings")]
    public float healValue = 20f; // Сколько здоровья восстанавливает орб

    [Header("Cooldown Settings")]
    public float cooldownValue = 10f; // Сколько времени сбрасывает кулдаун оружия
}