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

    //private InputAction _weapon1Action;
    //private InputAction _weapon2Action;
    //private InputAction _quickSwapAction;
    private float quickSwapTimer = 0f;

    public WeaponBase ActiveWeapon => (allWeapons != null && allWeapons.Count > 0) ? allWeapons[currentSlot] : null;
    public int CurrentSlot => currentSlot;
    public bool CanQuickSwap => quickSwapTimer <= 0f;

    private int lastSlot = -1; 

    private void Awake()
    {
        // Auto-assign PlayerMotor if not set
        if (playerMotor == null)
        {
            playerMotor = GetComponent<PlayerMotor>();
        }
    }

    private void Start()
    {
        if (playerMotor == null)
        {
            Debug.LogError("[WeaponManager] PlayerMotor reference missing!");
            return;
        }

        SetupInputActions();
        InitializeWeapons();
        if (allWeapons.Count > 0)
        {
            SetActiveWeapon(0);
        }
    }

    private void SetupInputActions()
    {
        var playerInput = playerMotor.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("[WeaponManager] PlayerInput component not found on PlayerMotor!");
            return;
        }
/*
        _weapon1Action = playerInput.actions.FindAction("1");
        _weapon2Action = playerInput.actions.FindAction("2");
        _quickSwapAction = playerInput.actions.FindAction("QuickSwap");

        if (_weapon1Action != null) _weapon1Action.performed += OnWeapon1;
        if (_weapon2Action != null) _weapon2Action.performed += OnWeapon2;
        if (_quickSwapAction != null) _quickSwapAction.performed += QuickSwap;
        */
    }

    private void InitializeWeapons()
    {
        foreach (var weapon in allWeapons)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(false);
                weapon.Initialize(playerMotor);
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
            playerMotor.activeWeapon = newWeapon;
            // Ensure reference is up to date (Initialize already called)
        }
    }

    public WeaponBase GetActiveWeapon()
    {
        return ActiveWeapon;
    }


    public void QuickSwap()
    {
        if (!CanQuickSwap) return;

        // Если мы еще ни разу не переключались (lastSlot == -1), 
        // или если текущее оружие и последнее — это одно и то же, ничего не делаем.
        if (lastSlot == -1 || lastSlot == currentSlot)
        {
            return;
        }

        // Логика: Переключаемся на тот слот, который был до этого
        int slotToSwitchTo = lastSlot;

        // Важно: Мы используем SwitchWeapon, чтобы обновить и currentSlot, и lastSlot
        SwitchWeapon(slotToSwitchTo);
        
        quickSwapTimer = quickSwapCooldown;
    }

    // Этот метод больше не нужен в старом виде, так как логика теперь в lastSlot
    // Но если вы хотите использовать его для обычного перебора (1-2-3), 
    // убедитесь, что SwitchWeapon вызывается корректно.
    private void Update()
    {
        if (quickSwapTimer > 0f)
            quickSwapTimer -= Time.deltaTime;
    }
}
