using UnityEngine;
using System.Collections.Generic;

public class SwordWeapon : WeaponBase
{
    [Header("Attack Settings")]
    public LayerMask enemyLayers; // Слой для врагов
    public float attackRange = 2f;
    public float attackRadius = 1f;
    public Transform attackPoint; // Drag here in Inspector! 🎯
    // Внутренняя логика нанесения урона (физика, триггеры и т.д.)


        // Этот метод вызывается из WeaponBase.HandleContinuousInput, когда таймер fireRate прошел и кнопка зажата.
    protected override void TryFire()
    {
        // Нам НЕ НУЖНО проверять Time.time здесь, так как это уже сделал базовый класс.
        // Нам НЕ НУЖНО проверять isOverheated здесь, так как это тоже сделал базовый класс.
        Debug.Log("[Sword] Swing animation/logic triggered");
        PerformAttack(data.baseDamage);
    }

    protected void PerformAttack(float damage)
    {
        if (attackPoint == null) 
        {
            Debug.LogError("[Sword] Attack point not assigned!", this);
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        List<EnemyBase> enemiesHit = new List<EnemyBase>();

        foreach (var collider in hitColliders)
        {
            EnemyBase enemy = collider.GetComponent<EnemyBase>();
            if (enemy != null && !enemiesHit.Contains(enemy))
            {
                enemy.TakeDamage(damage);
                enemiesHit.Add(enemy);
                // Добавь визуальный эффект попадания или звук здесь — для подтверждения урона
            }
        }

        Debug.Log($"[Sword] Attack! Damage: {damage}, Enemies hit: {enemiesHit.Count}");

        // 🔥 Применяем нагрев только если есть попадание и разрешено
        if (enemiesHit.Count > 0)
        {
            ApplyHeat(data.heatPerShot);
        }
    }

    // Этот метод вызывается из WeaponBase.HandleContinuousInput при зажатой ПКМ
    protected override void ExecuteSkill()
    {
        // В бумер-шутере блок — это состояние. 
        // Если мы попали сюда, значит таймер fireRate прошел.
        // Для меча "навык" (ПКМ) может быть либо мгновенным ударом, либо переключением режима в "Блок".
        Debug.Log("[Sword] Skill/Block Active");
        //Block_Enemy_Projectile();
    }
    // Дополнительно: если вы хотите, чтобы блок ПРЕКРАЩАЛСЯ, когда отпускают кнопку, вам может понадобиться переопределить HandleContinuousInput или добавить логику в Update.

/*
    protected void Block_Enemy_Projectile()
    {
        // Для меча "навык" — это блок/парирование.
        // Если мы здесь — значит таймер skillCooldown прошёл и оружие не перегрето (проверено в WeaponBase).
        
        Debug.Log("[Sword] Skill/Block Active");

        // Попытка парирования: проверяем, есть ли атакующий враг в радиусе блока
        if (playerMotor == null) return;

        // Примерный радиус блока — можно вынести в WeaponData
        float blockRadius = 2f;
        Collider[] hitColliders = Physics.OverlapSphere(playerMotor.transform.position, blockRadius, enemyLayers);

        bool wasBlocked = false;
        foreach (var collider in hitColliders)
        {
            EnemyBase enemy = collider.GetComponent<EnemyBase>();
            if (enemy != null && !wasBlocked)
            {
                // Проверяем направление атаки — для простоты считаем, что парирование срабатывает при близости
                // В реальном проекте можно добавить проверку угла между вектором взгляда игрока и направлением к врагу.
                
                // Если враг атакует (можно добавить флаг isAttacking), то блокируем
                if (enemy.TryBlockAttack())
                {
                    wasBlocked = true;
                    break;
                }
            }
        }

        if (wasBlocked)
        {
            Debug.Log("[Sword] Block successful!");
            
            // Нагрев при успешном парировании, если настроено
            if (data.skillUsesHeat && !isOverheated)
            {
                ApplyHeat(data.heatPerSkill);
            }
        }
    }
*/

    /// <summary>
    /// Вспомогательный метод для применения тепла (вынесен из WeaponBase, чтобы не дублировать логику).
    /// </summary>
    private void ApplyHeat(float amount)
    {
        currentHeat += amount;
        currentHeat = Mathf.Clamp(currentHeat, 0, data.overheatThreshold);
    }
}
