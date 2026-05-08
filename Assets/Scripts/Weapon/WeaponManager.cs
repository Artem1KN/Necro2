using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    //Fields: List<WeaponBase> allWeapons, int currentSlot, float quickSwapTimer.
    // Methods: SwitchWeapon(int slot), GetActiveWeapon().
    // Смысл: Подписывается на PlayerInput. Реализует логику "скидки" на нагрев при Quick Swap (Q). Если пушка не achieved, переключение блокируется.
}
