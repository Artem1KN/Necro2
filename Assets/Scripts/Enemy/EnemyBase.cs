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

    [Header("Audio")]
    public AudioClip hurtSfx;
    public AudioClip deathSfx;

    [Header("Events")]
    public Action<float, float> onHealthChanged;
    public Action onDeath;

    private float currentHP;
    private Vector3 lastHitDirection;

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

        if (hurtSfx != null && currentHP > 0)
            AudioManager.EnsureExists().PlaySfxAt(hurtSfx, transform.position);

        // Проверка смерти
        if (currentHP == 0)
        {
            Die();
        }

        return currentHP;
    }

    /// Variant used by weapons that know the hit direction (for ragdoll impulse).
    public float TakeDamage(float damage, Vector3 hitDirection)
    {
        lastHitDirection = hitDirection;
        return TakeDamage(damage);
    }

    private void Die()
    {
        SpawnEnergyOrb();
        onDeath?.Invoke();

        if (deathSfx != null)
            AudioManager.EnsureExists().PlaySfxAt(deathSfx, transform.position);

        var ragdoll = GetComponent<EnemyRagdollController>();
        if (ragdoll != null)
        {
            ragdoll.TriggerRagdoll(lastHitDirection);
        }
        else
        {
            Destroy(gameObject);
        }
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