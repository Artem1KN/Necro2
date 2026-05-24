using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class EnemyBase : MonoBehaviour, IDamagable
{
    [Header("Health Settings")]
    public float maxHP = 50f;

    [Header("Loot")]
    [Tooltip("Energy orb prefab spawned on death. Must contain EnergyOrb component.")]
    public GameObject energyOrbPrefab;

    [Tooltip("Orb config applied to the spawned orb (heal value, size, scale).")]
    public OrbData orbDropOnDeath;

    [Header("Events")]
    public Action<float, float> onHealthChanged;
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

    private void Die()
    {
        SpawnEnergyOrb();
        onDeath?.Invoke();
        Destroy(gameObject);
    }

    private void SpawnEnergyOrb()
    {
        if (energyOrbPrefab == null || orbDropOnDeath == null) return;

        var orbGo = Instantiate(energyOrbPrefab, transform.position, Quaternion.identity);
        if (orbGo.TryGetComponent<EnergyOrb>(out var orb))
        {
            orb.orbData = orbDropOnDeath;
        }

        if (orbDropOnDeath.scaleMultiplier > 0f && Mathf.Abs(orbDropOnDeath.scaleMultiplier - 1f) > 0.001f)
        {
            orbGo.transform.localScale *= orbDropOnDeath.scaleMultiplier;
        }
    }
}