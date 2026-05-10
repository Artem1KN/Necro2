using UnityEngine;
using System.Collections.Generic;

public class SwordWeapon : WeaponBase
{
    [Header("Attack Settings")]
    public LayerMask enemyLayers; // Слой для врагов
    public float attackRange = 2f;
    public float attackRadius = 1f;

    // Внутренняя логика нанесения урона (физика, триггеры и т.д.)
    protected void PerformAttack(float damage)
    {
        // Получаем все колайдеры в зоне атаки
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayers);
        
        List<EnemyBase> enemiesHit = new List<EnemyBase>();
        
        foreach (var collider in hitColliders)
        {
            EnemyBase enemy = collider.GetComponent<EnemyBase>();
            if (enemy != null && !enemiesHit.Contains(enemy))
            {
                // Наносим урон через IDamagable
                enemy.TakeDamage(damage);
                enemiesHit.Add(enemy);
            }
        }
        
        Debug.Log($"[Sword] Attack! Damage: {damage}, Enemies hit: {enemiesHit.Count}");
    }

    // Этот метод вызывается из WeaponBase.HandleContinuousInput, когда таймер fireRate прошел и кнопка зажата.
    protected override void TryFire()
    {
        // Нам НЕ НУЖНО проверять Time.time здесь, так как это уже сделал базовый класс.
        // Нам НЕ НУЖНО проверять isOverheated здесь, так как это тоже сделал базовый класс.
        
        Debug.Log("[Sword] Swing animation/logic triggered");
        PerformAttack(data.baseDamage);
    }

    // Этот метод вызывается из WeaponBase.HandleContinuousInput при зажатой ПКМ
    protected override void ExecuteSkill()
    {
        // В бумер-шутере блок — это состояние. 
        // Если мы попали сюда, значит таймер fireRate прошел.
        // Для меча "навык" (ПКМ) может быть либо мгновенным ударом, либо переключением режима в "Блок".
        
        Debug.Log("[Sword] Skill/Block Active");
    }

    // Дополнительно: если вы хотите, чтобы блок ПРЕКРАЩАЛСЯ, когда отпускают кнопку, вам может понадобиться переопределить HandleContinuousInput или добавить логику в Update.
}
