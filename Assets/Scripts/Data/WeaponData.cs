using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Necro2/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("General Info")]
    public string weaponName;
    public bool isAchieved = false;

    [Header("Damage Settings")]
    public float baseDamage = 10f;
    public float optimalHeatMultiplier = 1.5f; // Бонус в зоне 70-90%
    public float fireRate = 0.2f;

    [Header("Heat Mechanics")]
    public float heatPerShot = 5f;
    public float passiveCoolingRate = 10f; // Базовое охлаждение в сек
    public float activeCoolingBonus = 1.5f; // На сколько быстрее остывает в руках
    public bool canBeBlocked = true; // Для меча будет false
    public float overheatThreshold = 100f;
    public float recoveryThreshold = 50f; // До скольки надо остыть после перегрева

    [Header("Optimal Zone")]
    public float optimalZoneStart = 70f;
    public float optimalZoneEnd = 90f;

    [Header("Visuals")]
    public GameObject weaponPrefab;
    // Сюда можно добавить настройки отдачи для DOTween
    public float recoilStrength = 0.1f;
    public float recoilDuration = 0.1f;
}