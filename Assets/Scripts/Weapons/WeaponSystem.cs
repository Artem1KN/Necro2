using UnityEngine;
using System.Collections.Generic;

public class WeaponSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GlobalCombatConfig combatConfig;
    private List<WeaponBase> weapons = new List<WeaponBase>();
    private int currentWeaponIndex = -1;
    private WeaponBase lastActiveWeapon;
    private HeatController heatController;

    public WeaponBase CurrentWeapon => currentWeaponIndex != -1 ? weapons[currentWeaponIndex] : null;
    public bool HasWeapons => weapons.Count > 0;

    private void Awake()
    {
        heatController = GetComponentInParent<HeatController>();
        weapons.AddRange(GetComponentsInChildren<WeaponBase>());
        
        if (weapons.Count > 0)
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                if (weapons[i].Data.achieved)
                {
                    SelectWeapon(i);
                    break;
                }
            }
        }
    }

    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return;
        if (!weapons[index].Data.achieved) return;

        lastActiveWeapon = CurrentWeapon;
        currentWeaponIndex = index;
    }

    public void QuickSwap()
    {
        int prevIndex = -1;
        for (int i = currentWeaponIndex - 1; i >= 0; i--)
        {
            if (weapons[i].Data.achieved)
            {
                prevIndex = i;
                break;
            }
        }

        if (prevIndex != -1)
        {
            SelectWeapon(prevIndex);
            if (heatController != null)
            {
                heatController.StartQuickSwapBonus();
            }
        }
    }

    public void SetWeaponUnlocked(string weaponName, bool unlocked)
    {
        foreach (var weapon in weapons)
        {
            if (weapon.Data.weaponName == weaponName)
            {
                // For now, we follow the user's request to modify the asset directly.
                weapon.Data.achieved = unlocked;
                break;
            }
        }
    }
}