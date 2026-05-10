using UnityEngine;

public interface IDamagable
{
    /// <summary>
    /// Наносит урон сущности. Возвращает оставшееся здоровье.
    /// </summary>
    float TakeDamage(float damage);
    
    /// <summary>
    /// Текущее здоровье.
    /// </summary>
    float CurrentHP { get; }
    
    /// <summary>
    /// Максимальное здоровье.
    /// </summary>
    float MaxHP { get; }
}

/// <summary>
/// Данные для EnergyOrb - объекта, появляющегося при смерти врага.
/// </summary>
/*
[CreateAssetMenu(fileName = "NewOrbData", menuName = "Necro2/Orb Data")]
public class OrbData : ScriptableObject
{
    [Header("Energy Orb Settings")]
    public float healValue = 10f;
    public float cooldownValue = 5f;
    public float speed = 5f;
}
*/