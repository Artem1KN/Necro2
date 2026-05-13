using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class PlayerHealth : MonoBehaviour, IDamagable
{
    [Header("Health Settings")]
    public float maxHP = 100f;
    [Header("Events")]
    public Action<float, float> onHealthChanged; // current, max
    public Action onDeath;

    private float currentHP;
    private bool isDead = false;

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;

    private void Awake()
    {
        currentHP = maxHP;
    }

    /// <summary>
    /// Наносит урон игроку.
    /// </summary>
    public float TakeDamage(float damage)
    {
        if (isDead || damage <= 0) return currentHP;

        currentHP -= damage;
        if (currentHP < 0) currentHP = 0;

        onHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP == 0 && !isDead)
        {
            isDead = true;
            Die();
        }

        Debug.Log($"[Player] {currentHP}");
        return currentHP;
    }

    /// <summary>
    /// Лечит игрока.
    /// </summary>
    public void Heal(float amount)
    {
        if (amount <= 0 || isDead) return;

        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        onHealthChanged?.Invoke(currentHP, maxHP);
    }

    /// <summary>
    /// Смерть игрока.
    /// </summary>
    private void Die()
    {
        // Блокируем физику
        /*
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
*/
        onDeath?.Invoke();
        
        // Опционально: отключаем управление
        //var input = GetComponent<PlayerInput>() as MonoBehaviour; // замените на ваш класс управления
        //if (input != null) input.enabled = false;

        Debug.Log("[Player] Died!");
    }

    private void OnDestroy()
    {
        onHealthChanged = null;
        onDeath = null;
    }
}
