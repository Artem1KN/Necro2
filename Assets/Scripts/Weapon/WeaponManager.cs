using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] private List<WeaponBase> allWeapons = new List<WeaponBase>();
    [SerializeField] private int currentSlot = 0;
    [SerializeField] private float quickSwapCooldown = 0.3f;

    [Header("References")]
    [SerializeField] private PlayerMotor playerMotor;
    [SerializeField] private TestPlayerController testPlayer;
    private float quickSwapTimer = 0f;

    public WeaponBase ActiveWeapon => (allWeapons != null && allWeapons.Count > 0) ? allWeapons[currentSlot] : null;
    public int CurrentSlot => currentSlot;
    public bool CanQuickSwap => quickSwapTimer <= 0f;
    public IReadOnlyList<WeaponBase> AllWeapons => allWeapons;

    private MonoBehaviour ActiveController => playerMotor != null ? (MonoBehaviour)playerMotor : testPlayer;

    private float GetCurrentSpeed()
    {
        if (playerMotor != null) return playerMotor.currentSpeed;
        if (testPlayer != null) return testPlayer.currentSpeed;
        return 0f;
    }

    private int lastSlot = -1; 

    private void Awake()
    {
        if (playerMotor == null) playerMotor = GetComponent<PlayerMotor>();
        if (testPlayer == null) testPlayer = GetComponent<TestPlayerController>();
    }

    private void Start()
    {
        if (ActiveController == null)
        {
            Debug.LogError("[WeaponManager] No player controller assigned (PlayerMotor or TestPlayerController).", this);
            return;
        }

        InitializeWeapons();
        if (allWeapons.Count > 0)
        {
            SetActiveWeapon(0);
        }
    }

    private void InitializeWeapons()
    {
        var controller = ActiveController;
        foreach (var weapon in allWeapons)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(false);
                weapon.Initialize(controller);
            }
        }
    }

    public void SwitchWeapon(int slot)
    {
        if (slot < 0 || slot >= allWeapons.Count)
        {
            Debug.LogWarning($"[WeaponManager] Invalid weapon slot: {slot}");
            return;
        }

        var targetWeapon = allWeapons[slot];
        if (targetWeapon == null)
        {
            Debug.LogWarning($"[WeaponManager] Weapon at slot {slot} is null.");
            return;
        }

        if (!targetWeapon.data.isAchieved)
        {
            Debug.LogWarning($"[WeaponManager] Weapon '{targetWeapon.data.weaponName}' is not achieved yet.");
            return;
        }

        lastSlot = currentSlot; 

        SetActiveWeapon(slot);
        Debug.Log($"[WeaponManager] Switched to {targetWeapon.data.weaponName}");
    }

    private void SetActiveWeapon(int slot)
    {
        // Deactivate previous weapon
        if (currentSlot >= 0 && currentSlot < allWeapons.Count && allWeapons[currentSlot] != null)
        {
            allWeapons[currentSlot].gameObject.SetActive(false);
        }


        currentSlot = slot;
        var newWeapon = allWeapons[currentSlot];
        if (newWeapon != null)
        {
            newWeapon.gameObject.SetActive(true);
            if (playerMotor != null) playerMotor.activeWeapon = newWeapon;
            if (testPlayer != null) testPlayer.activeWeapon = newWeapon;
        }
    }

    public WeaponBase GetActiveWeapon()
    {
        return ActiveWeapon;
    }

    public void QuickSwap()
    {
        if (!CanQuickSwap) return;
        // Если мы еще ни разу не переключались (lastSlot == -1), или если текущее оружие и последнее — это одно и то же, ничего не делаем.
        if (lastSlot == -1 || lastSlot == currentSlot)  return;
        // Логика: Переключаемся на тот слот, который был до этого
        int slotToSwitchTo = lastSlot;
        // Важно: Мы используем SwitchWeapon, чтобы обновить и currentSlot, и lastSlot
        SwitchWeapon(slotToSwitchTo);
        quickSwapTimer = quickSwapCooldown;
    }

    private void Update()
    {
        if (quickSwapTimer > 0f)
            quickSwapTimer -= Time.deltaTime;

        // НОВАЯ ЛОГИКА: Обновление нагрева для всех пушек
        UpdateAllWeaponsHeat();
    }

    private void UpdateAllWeaponsHeat()
    {
        if (allWeapons == null || ActiveController == null) return;

        float speed = GetCurrentSpeed();

        foreach (var weapon in allWeapons)
        {
            if (weapon == null) continue;
            bool isActive = (weapon == ActiveWeapon);
            weapon.Tick(Time.deltaTime, isActive, speed);
        }
    }
}
