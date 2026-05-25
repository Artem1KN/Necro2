using UnityEngine;

public enum OrbSize
{
    Small,
    Medium,
    Large
}

[CreateAssetMenu(fileName = "OrbData", menuName = "Gameplay/Orb Data")]
public class OrbData : ScriptableObject
{
    [Header("Identity")]
    public OrbSize size = OrbSize.Medium;

    [Header("Heal Settings")]
    public float healValue = 20f;

    [Header("Cooldown Settings")]
    public float cooldownValue = 10f;

    [Header("Visual")]
    [Tooltip("Local scale multiplier applied to the orb prefab when spawned.")]
    public float scaleMultiplier = 1f;
}
