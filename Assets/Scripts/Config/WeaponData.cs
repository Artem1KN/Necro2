using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Necro2/Config/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("General Info")]
    public string weaponName;
    public bool isMelee = false;
    public bool achieved = false;

    [Header("Damage Settings")]
    public float baseDamage = 10f;
    [Tooltip("The multiplier for damage based on heat level. Using an AnimationCurve allows for non-linear scaling.")]
    public AnimationCurve damageMultiplierCurve = AnimationCurve.Linear(0, 1, 1, 2);

    [Header("Heat Settings")]
    public float heatPerShot = 5f;
    public float heatPerSecond = 2f;
    public float coolingRateBase = 10f;
    [Tooltip("Maximum heat percentage (e.g., 1.0 for 100%)")]
    public float maxHeatPercent = 1.0f;

    [Header("Weapon Type Specifics")]
    public GameObject projectilePrefab; // For projectile weapons
    public float attackRange = 50f;     // For hitscan weapons
    public float attackCooldown = 0.2f; // Time between shots
}