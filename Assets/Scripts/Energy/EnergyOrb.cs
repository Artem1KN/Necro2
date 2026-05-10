using UnityEngine;
using System;

[RequireComponent(typeof(Collider))]
public class EnergyOrb : MonoBehaviour
{
    [Header("Settings")]
    public OrbData orbData;
    public float collectDistance = 1f; // Дистанция сбора орба

    private Action<float, float> onHeal; // action(current, max)
    private Action<float> onCooldown; // action(amount)

    /// <summary>
    /// Установка обработчиков для лечения и кулдауна.
    /// </summary>
    public void Setup(Action<float, float> healCallback, Action<float> cooldownCallback)
    {
        onHeal = healCallback;
        onCooldown = cooldownCallback;
    }

    private void Update()
    {
        // Автоматический сбор орба игроком
        PlayerHealth player = FindObjectOfType<PlayerHealth>();
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance <= collectDistance)
            {
                CollectOrb(player);
            }
        }
    }

    /// <summary>
    /// Сбор орба игроком.
    /// </summary>
    private void CollectOrb(PlayerHealth player)
    {
        // Лечение
        float healAmount = orbData.healValue;
        if (player != null && healAmount > 0)
        {
            float currentHP = player.CurrentHP;
            float maxHP = player.MaxHP;
            float newHP = Math.Min(currentHP + healAmount, maxHP);
            
            // Вычисляем реальное количество лечения
            float actualHeal = newHP - currentHP;
            if (actualHeal > 0)
            {
                player.Heal(actualHeal);
            }
        }

        // Кулдаун оружия
        /*
        float cooldownAmount = orbData.cooldownValue;
        if (cooldownAmount > 0)
        {
            WeaponManager weaponManager = FindObjectOfType<WeaponManager>();
            if (weaponManager != null && weaponManager.currentWeapon != null)
            {
                weaponManager.currentWeapon.AddCooldown(cooldownAmount);
                onCooldown?.Invoke(cooldownAmount);
            }
        }
        */

        // Уничтожаем орб после сбора
        Destroy(gameObject);
    }

    /// <summary>
    /// Отображает визуальный эффект при сборе.
    /// </summary>
    private void OnParticleSystemExit()
    {
        // Дополнительная логика при уничтожении (если нужно)
    }
}