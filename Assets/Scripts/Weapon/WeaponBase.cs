using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData data;
    
    [Header("Current State")]
    public float currentHeat = 0f;
    public bool isOverheated = false;
    protected float lastFireTime;

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

    protected virtual void OnEnable()
    {
        // Ищем мотор только один раз при подключении оружия
        if (playerMotor == null)
            playerMotor = GetComponentInParent<PlayerMotor>();
        
        if (playerMotor != null)
        {
            playerMotor.OnAttackPerformed += HandleAttackPerformed;
            playerMotor.OnSkillPerformed += HandleSkillPerformed;
        }
    }

    protected virtual void OnDisable()
    {
        if (playerMotor != null)
        {
            playerMotor.OnAttackPerformed += HandleAttackPerformed;
            playerMotor.OnSkillPerformed -= HandleSkillPerformed;
        }
    }
    private void HandleAttackPerformed()
    {
        TryFire();
    }
    private void HandleSkillPerformed()
    {
        ExecuteSkill();
    }

    // Можно добавить метод для "привязки" оружия при старте
    public void Initialize(PlayerMotor motor)
    {
        playerMotor = motor;
    }

    // Абстрактные методы, которые реализуют конкретные пушки
    protected abstract void TryFire();
    protected abstract void ExecuteSkill(); // Заготовка под ПКМ
}