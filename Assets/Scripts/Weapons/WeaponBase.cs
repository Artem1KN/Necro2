using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected WeaponData weaponData;
    protected float currentHeat = 0f;
    protected bool isOverheated = false;
    protected float lastAttackTime;

    public WeaponData Data => weaponData;
    public float CurrentHeatPercent => weaponData.maxHeatPercent > 0 ? currentHeat / weaponData.maxHeatPercent : 0;
    public bool IsOverheated => isOverheated;

    protected virtual void Update()
    {
        HandlePassiveCooling();
        HandleHeatPerSecond();
    }

    protected virtual void HandlePassiveCooling()
    {
        if (currentHeat <= 0) return;

        // Basic decay
        currentHeat -= weaponData.coolingRateBase * Time.deltaTime;
        if (currentHeat < 0) currentHeat = 0;
        
        if (isOverheated && currentHeat < weaponData.maxHeatPercent)
        {
            isOverheated = false;
        }
    }

    protected virtual void HandleHeatPerSecond()
    {
        // Heat increases over time if not shooting? 
        // Based on GDD, we'll handle specific heat spikes in Attack() and cooling in HeatController.
    }

    public virtual void AddHeat(float amount)
    {
        if (weaponData.isMelee) return;

        currentHeat += amount;
        if (currentHeat >= weaponData.maxHeatPercent)
        {
            currentHeat = weaponData.maxHeatPercent;
            isOverheated = true;
        }
    }

    public virtual void ApplyExtraCooling(float amount)
    {
        if (weaponData.isMelee) return;
        currentHeat -= amount;
        if (currentHeat < 0) currentHeat = 0;
        if (isOverheated && currentHeat < weaponData.maxHeatPercent)
        {
            isOverheated = false;
        }
    }

    public virtual void ResetHeat()
    {
        currentHeat = 0;
        isOverheated = false;
    }

    public abstract void Attack();
}