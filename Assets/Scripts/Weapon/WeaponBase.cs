using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    public WeaponData data;
    
    [Header("Current State")]
    public float currentHeat = 0f;
    public bool isOverheated = false;
    protected float lastFireTime;
    protected float lastSkillTime;

    [HideInInspector] public PlayerMotor playerMotor;
    [HideInInspector] public TestPlayerController testPlayer;

    protected void AddRecoilFromData()
    {
        if (data == null) return;
        if (playerMotor != null) playerMotor.AddRecoil(data.recoilStrength, data.recoilDuration);
        else if (testPlayer != null) testPlayer.AddRecoil(data.recoilStrength, data.recoilDuration);
    }

    protected void PlayFireSfx()
    {
        if (data == null || data.fireSfx == null) return;
        AudioManager.EnsureExists().PlaySfx2D(data.fireSfx);
    }

    protected void PlaySkillSfx()
    {
        if (data == null || data.skillSfx == null) return;
        AudioManager.EnsureExists().PlaySfx2D(data.skillSfx);
    }

    // Флаги состояния, которые мы будем получать из Motor
    private bool _isAttackHeld;
    private bool _isSkillHeld;

    /// <summary>
    /// Основной метод обновления состояния оружия. 
    /// Вызывается WeaponManager'ом каждый кадр для ВСЕХ пушек в списке.
    /// </summary>
    public void Tick(float deltaTime, bool isCurrentlyActive, float playerSpeed)
    {
        if (data == null) return;
        HandlePassiveCooling(deltaTime, isCurrentlyActive, playerSpeed);
    }

     /// <summary>
    /// Этот метод вызывается из PlayerMotor.Update каждый кадр.
    /// </summary>
    public void HandleContinuousInput(bool isAttackPressed, bool isSkillPressed)
    {
        if (data == null) return;

        _isAttackHeld = isAttackPressed;
        _isSkillHeld = isSkillPressed;

        if (_isAttackHeld && !isOverheated)
        {
            if (Time.time >= lastFireTime + data.fireRate)
            {
                TryFire();
                lastFireTime = Time.time;
            }
        }

        if (_isSkillHeld && !isOverheated)
        {
            if (Time.time >= lastSkillTime + data.skillCooldown)
            {
                ExecuteSkill();
                lastSkillTime = Time.time;
            }
        }
    }

    private void HandlePassiveCooling(float deltaTime, bool isCurrentlyActive, float playerSpeed)
    {
        if (currentHeat <= 0) return;

        // Логика: охлаждение зависит от скорости игрока
        float speedFactor = Mathf.Max(1f, playerSpeed);
        
        // Базовая скорость остывания
        float coolingPerSecond = data.passiveCoolingRate * speedFactor;

        // Если оружие активно (в руках), можно применить бонус к охлаждению, если он есть в логике
        if (isCurrentlyActive)
        {
            // Если вы захотите использовать activeCoolingBonus из WeaponData:
            // coolingPerSecond *= data.activeCoolingBonus; 
        }

        float cooling = coolingPerSecond * deltaTime;
        currentHeat -= cooling;
        currentHeat = Mathf.Clamp(currentHeat, 0, data.overheatThreshold);

        if (isOverheated && currentHeat <= data.recoveryThreshold)
        {
            isOverheated = false;
            // Event: Weapon Ready
        }
    }

    protected virtual void OnEnable()
    {
        if (playerMotor == null) playerMotor = GetComponentInParent<PlayerMotor>();
        if (testPlayer == null) testPlayer = GetComponentInParent<TestPlayerController>();
    }

    public void Initialize(MonoBehaviour controller)
    {
        if (controller is PlayerMotor m) playerMotor = m;
        else if (controller is TestPlayerController t) testPlayer = t;
    }

    // Абстрактные методы, которые реализуют конкретные пушки
    protected abstract void TryFire();
    protected abstract void ExecuteSkill();
}