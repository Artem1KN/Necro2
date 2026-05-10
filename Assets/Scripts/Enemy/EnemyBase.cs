using UnityEngine;
using System;

/// <summary>
/// Базовый класс для врагов. Реализует IDamagable.
/// </summary>
[RequireComponent(typeof(Collider))]
public class EnemyBase : MonoBehaviour, IDamagable
{
    [Header("Health Settings")]
    public float maxHP = 50f;
    
    [Header("Events")]
    public Action<float, float> onHealthChanged; // current, max
    public Action onDeath;

    private float currentHP;

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// Наносит урон врагу.
    /// </summary>
    public float TakeDamage(float damage)
    {
        if (damage <= 0) return currentHP;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        onHealthChanged?.Invoke(currentHP, maxHP);

        // Проверка смерти
        if (currentHP == 0)
        {
            Die();
        }

        return currentHP;
    }

    /// <summary>
    /// Смерть врага.
    /// </summary>
    private void Die()
    {
        onDeath?.Invoke();
        
        // Уничтожаем объект после смерти
        Destroy(gameObject);
    }
}