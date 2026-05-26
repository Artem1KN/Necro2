using System.Collections.Generic;
using UnityEngine;

public class AssaultRifleWeapon : WeaponBase
{
    [Header("Attack Settings")]
    public LayerMask enemyLayers;              // Слой врагов
    public float maxRange = 10f;               // Дальность hitscan
    public float spreadAngle = 2f;             // Разброс направления выстрела (в градусах)
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;

    protected override void TryFire()
    {
        Ray ray = GetRayWithSpread();
        if (WeaponRaycastUtil.RaycastSkippingPlayer(ray.origin, ray.direction, maxRange, enemyLayers, out RaycastHit hit))
        {
            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>() ?? hit.collider.GetComponentInParent<EnemyBase>();
            if (enemy != null)
            {
                float damage = data.baseDamage;
                if (!isOverheated && currentHeat >= data.optimalZoneStart && currentHeat <= data.optimalZoneEnd)
                    damage *= data.optimalHeatMultiplier;

                enemy.TakeDamage(damage);

                if (hitEffectPrefab != null)
                {
                    var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(fx, 1f);
                }
            }
        }

        

        // 🔥 Нагреваем оружие — ВСЕГДА при выстреле (не только при попадании!)
        ApplyHeat(data.heatPerShot);

        AddRecoilFromData();

        // ✨ Muzzle flash (опционально)
        if (muzzleFlashPrefab != null)
        {
            Transform muzzle = GetMuzzleTransform();
            if (muzzle != null)
            {
                ParticleSystem flash = Instantiate(muzzleFlashPrefab, muzzle.position, Quaternion.identity).GetComponent<ParticleSystem>();
                if (flash != null) flash.Play();
                Destroy(flash.gameObject, 0.1f);
            }
        }
    }

    protected override void ExecuteSkill()
    {
        // Винтовка пока не имеет навыка — можно оставить пустым или добавить burst/zoom
        Debug.Log("[AssaultRifle] Skill not implemented");
    }

    /// <summary>
    /// Получает луч с учетом разброса.
    /// </summary>
    private Ray GetRayWithSpread()
    {
        var cam = Camera.main;
        Vector3 origin = cam != null ? cam.transform.position : transform.position;
        Vector3 direction = cam != null ? cam.transform.forward : transform.forward;

        float spreadX = Random.Range(-data.spreadAngle, data.spreadAngle);
        float spreadY = Random.Range(-data.spreadAngle, data.spreadAngle);
        Vector3 spreadDirection = Quaternion.Euler(spreadY, spreadX, 0) * direction;
        return new Ray(origin, spreadDirection);
    }

    /// <summary>
    /// Место вылета пули (обычно — точка на конце ствола)
    /// </summary>
    private Transform GetMuzzleTransform()
    {
        // Можно добавить public Transform muzzlePoint в inspectorApplyHeat
        if (transform.childCount > 0)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.ToLower().Contains("muzzle"))
                    return transform.GetChild(i);
            }
        }
        // Fallback — сам трансформ оружия
        return transform;
    }

    /// <summary>
    /// Применяет тепло и обновляет состояние перегрева.
    /// Вынесено из WeaponBase, чтобы переопределять при необходимости (но пока используем базовое ApplyHeat).
    /// </summary>
    private void ApplyHeat(float amount)
    {
        currentHeat += amount;
        currentHeat = Mathf.Clamp(currentHeat, 0f, data.overheatThreshold);

        if (!isOverheated && currentHeat >= data.overheatThreshold)
        {
            isOverheated = true;
            // UI event: "Overheated!"
            Debug.LogWarning("[AssaultRifle] OVERHEATED!");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        // Можно добавить cleanup эффектов при включении/выключении, если нужно
    }
}
