using UnityEngine;

public class CrossbowWeapon : WeaponBase
{
    [Header("Projectile Settings")]
    public GameObject arrowPrefab;      // Префаб стрелы с компонентом ArrowProjectile
    public Transform spawnPoint;        // Точка вылета (дуга арбалета)
    public ForceMode launchForce = ForceMode.Impulse;

    [Header("Visuals")]
    public GameObject muzzleFlashPrefab; 
    public ParticleSystem hitEffect;

    protected override void TryFire()
    {
        Debug.Log("[Crossbow] Firing Bolt!");

        // 1. Создаем снаряд
        if (arrowPrefab != null && spawnPoint != null)
        {
            GameObject arrow = Instantiate(arrowPrefab, spawnPoint.position, spawnPoint.rotation);
            ArrowProjectile projectile = arrow.GetComponent<ArrowProjectile>();

            if (projectile != null)
            {
                // Передаем урон из WeaponData в снаряд
                projectile.damage = data.baseDamage;
                
                // Если нужно добавить физический импульс (если скорость не задана в Awake)
                Rigidbody rb = arrow.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(spawnPoint.forward * 10f, launchForce);
                }
            }
        }

        // 2. Нагрев оружия (Арбалет может нагреваться от частоты выстрелов/натяжения тетивы)
        ApplyHeat(data.heatPerShot);

        // 3. Визуал: Вспышка или звук натяжения
        if (muzzleFlashPrefab != null)
        {
            TriggerMuzzleFlash();
        }

        AddRecoilFromData();
    }

    protected override void ExecuteSkill()
    {
        // Пример навыка: "Heavy Bolt" - выстрел мощной стрелой с увеличенным уроном
        Debug.Log("[Crossbow] Skill: Heavy Bolt Activated!");
        
        // Здесь можно реализовать логику временного усиления следующего выстрела
        // Или мгновенный выстрел сверхмощным снарядом
    }

    private void TriggerMuzzleFlash()
    {
        Transform muzzle = GetMuzzleTransform();
        if (muzzle != null)
        {
            GameObject flashObj = Instantiate(muzzleFlashPrefab, muzzle.position, muzzle.rotation);
            ParticleSystem ps = flashObj.GetComponent<ParticleSystem>();
            if (ps != null) ps.Play();
            Destroy(flashObj, 0.2f);
        }
    }

    private void ApplyHeat(float amount)
    {
        currentHeat += amount;
        currentHeat = Mathf.Clamp(currentHeat, 0f, data.overheatThreshold);
        if (!isOverheated && currentHeat >= data.overheatThreshold)
        {
            isOverheated = true;
            Debug.LogWarning("[Crossbow] Tension too high! Overheated!");
        }
    }

    private Transform GetMuzzleTransform()
    {
        // Ищем дочерний объект с именем "Muzzle" или возвращаем текущий
        foreach (Transform child in transform)
        {
            if (child.name.ToLower().Contains("muzzle")) return child;
        }
        return transform;
    }
}
