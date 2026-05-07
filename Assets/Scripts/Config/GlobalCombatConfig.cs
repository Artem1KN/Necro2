using UnityEngine;

[CreateAssetMenu(fileName = "GlobalCombatConfig", menuName = "Necro2/Config/GlobalCombatConfig")]
public class GlobalCombatConfig : ScriptableObject
{
    [Header("Quick Swap Settings")]
    [Tooltip("Duration of the heat discount after weapon swap in seconds")]
    public float quickSwapDuration = 3f;

    [Header("Cooling Settings")]
    [Tooltip("Multiplier for passive cooling based on player movement speed")]
    public float speedBasedCoolingMultiplier = 1.5f;
    
    [Header("Visuals")]
    public Color coolingOrbColor = new Color(0f, 0.5f, 1f, 1f); // Blue
    public Color healingOrbColor = new Color(0f, 1f, 0f, 1f);  // Green
}