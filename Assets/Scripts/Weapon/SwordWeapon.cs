using UnityEngine;
using UnityEngine.InputSystem;

public class SwordWeapon : WeaponBase
{
    protected override void ShootLogic(float damage)
    {
        // Логика взмаха мечом (например, OverlapSphere)
        Debug.Log($"Sword Attack! Damage: {damage}");
        // Здесь DOTween анимация взмаха
    }

    protected override void ExecuteSkill()
    {
        // Логика блока/отражения
        Debug.Log("Sword Blocking...");
    }

    public override void OnAttack(InputValue value)
    {
        // Меч обычно не стреляет очередью, переопределяем под одиночные нажатия
        if (value.isPressed) TryFire();
    }
}