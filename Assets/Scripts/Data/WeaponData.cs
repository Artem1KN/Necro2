using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Necro2/Weapon Data")]
public class WeaponData : ScriptableObject
{
    [Header("General Info")]
    public string weaponName;
    public bool isAchieved = false;

    [Header("Damage Settings")]
    public float baseDamage = 10f;
    public float optimalHeatMultiplier = 1.5f; // Бонус урона при перегреве в зоне 70–90%
    public float fireRate = 0.2f;             // Интервал между выстрелами/атаками (сек)

    [Header("Heat Mechanics")]
     public float heatPerShot = 5f;            // ✅ добавлено: нагрев за *одну* атаку
     public bool appliesToMeleeOnlyOnHit = false; // ⭐ НОВОЕ — меч нагревается только при попадании (а не при каждом вызове TryFire)
     public float passiveCoolingRate = 10f;    // Охлаждение за секунду (например: 2 = теряем 2 единицы тепла в секунду)
    public float activeCoolingBonus = 1.5f;   // Уже используется в HandlePassiveCooling как множитель скорости охлаждения
    public bool canBeBlocked = true;          // ⭐ Меч: false — он не блокируется, но может блокировать/парировать
    public float overheatThreshold = 100f;
    public float recoveryThreshold = 50f;

    [Header("Optimal Zone")]
    public float optimalZoneStart = 70f;
    public float optimalZoneEnd = 90f;

    [Header("Skill Settings")]
    public bool skillUsesHeat = false;        // ⭐ НОВОЕ — блок/парирование может не нагревать (или нагревать, но отдельно)
    public float heatPerSkill = 0f;           // Если skillUsesHeat == true
    public float skillCooldown = 1.5f;        // ⭐ НОВОЕ — задержка между использованием навыков (для меча: парирование/блок)

    [Header("Visuals")]
    public GameObject weaponPrefab;
    public float recoilStrength = 0.1f;
    public float recoilDuration = 0.1f;
    public float spreadAngle = 5f;
}