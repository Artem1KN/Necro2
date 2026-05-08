using UnityEngine;
using UnityEngine.InputSystem;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData data;
    
    [Header("Current State")]
    public float currentHeat = 0f;
    public bool isOverheated = false;
    protected float lastFireTime;

    // Ссылка на мотор для расчета охлаждения от скорости (назначить через WeaponManager)
    [HideInInspector] public PlayerMotor playerMotor;

    protected virtual void Update()
    {
        HandlePassiveCooling();
    }

    private void HandlePassiveCooling()
    {
        if (currentHeat <= 0) return;

        // Логика: охлаждение зависит от скорости игрока
        float speedFactor = (playerMotor != null) ? Mathf.Max(1f, playerMotor.currentSpeed) : 1f;
        
        // Активное оружие остывает быстрее (activeCoolingBonus)
        float cooling = data.passiveCoolingRate * speedFactor * Time.deltaTime;
        
        currentHeat -= cooling;
        currentHeat = Mathf.Clamp(currentHeat, 0, data.overheatThreshold);

        if (isOverheated && currentHeat <= data.recoveryThreshold)
        {
            isOverheated = false;
            // Здесь можно вызвать ивент для UI: "Оружие готово"
        }
    }

    // Обработка ЛКМ через Unity Input System (SendMessage или PlayerInput component)
    public virtual void OnAttack(InputValue value)
    {
        if (value.isPressed) 
        {
            InvokeRepeating(nameof(TryFire), 0, data.fireRate);
        }
        else 
        {
            CancelInvoke(nameof(TryFire));
        }
    }

    // Обработка ПКМ
    public virtual void OnSkill(InputValue value)
    {
        if (value.isPressed)
        {
            ExecuteSkill();
        }
    }

    protected void TryFire()
    {
        if (!data.isAchieved) return;
        if (isOverheated && data.canBeBlocked) return;
        if (Time.time < lastFireTime + data.fireRate) return;

        lastFireTime = Time.time;
        
        // Рассчитываем урон с учетом нагрева
        float finalDamage = CalculateDamage();
        
        ShootLogic(finalDamage);

        // Нагрев
        if (data.canBeBlocked || currentHeat < data.overheatThreshold)
        {
            currentHeat += data.heatPerShot;
        }

        if (currentHeat >= data.overheatThreshold && data.canBeBlocked)
        {
            isOverheated = true;
            CancelInvoke(nameof(TryFire));
        }
    }

    private float CalculateDamage()
    {
        if (currentHeat >= data.optimalZoneStart && currentHeat <= data.optimalZoneEnd)
        {
            return data.baseDamage * data.optimalHeatMultiplier;
        }
        return data.baseDamage;
    }

    // Абстрактные методы, которые реализуют конкретные пушки
    protected abstract void ShootLogic(float damage);
    protected abstract void ExecuteSkill(); // Заготовка под ПКМ
}