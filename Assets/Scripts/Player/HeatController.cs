using UnityEngine;

public class HeatController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponSystem weaponSystem;
    [SerializeField] private GlobalCombatConfig combatConfig;
    [SerializeField] private PlayerMotor playerMotor;

    private float quickSwapTimer = 0f;

    private void Update()
    {
        if (weaponSystem == null || weaponSystem.CurrentWeapon == null) return;

        WeaponBase activeWeapon = weaponSystem.CurrentWeapon;

        // Handle Quick Swap Bonus timer
        if (quickSwapTimer > 0)
        {
            quickSwapTimer -= Time.deltaTime;
            // Apply extra cooling during quick swap period
            activeWeapon.ApplyExtraCooling(Time.deltaTime * 2f); 
        }

        // Handle Passive Cooling based on movement speed
        HandleSpeedBasedCooling(activeWeapon);
    }

    private void HandleSpeedBasedCooling(WeaponBase weapon)
    {
        if (playerMotor == null) return;

        float currentSpeed = playerMotor.GetComponent<Rigidbody>().linearVelocity.magnitude;
        
        // If moving, apply extra cooling multiplier from config
        if (currentSpeed > 0.1f)
        {
            float coolingAmount = combatConfig.speedBasedCoolingMultiplier * Time.deltaTime;
            weapon.ApplyExtraCooling(coolingAmount);
        }
    }

    public void ApplyKillCooling(WeaponBase weapon)
    {
        if (weapon == null) return;
        
        // Direct cooling on kill - as per GDD, only active weapon
        float burstAmount = 20f; 
        weapon.ApplyExtraCooling(burstAmount);
        
        Debug.Log($"Kill Cooling applied to {weapon.Data.weaponName}");
    }
    
    public void StartQuickSwapBonus()
    {
        quickSwapTimer = combatConfig.quickSwapDuration;
    }
}