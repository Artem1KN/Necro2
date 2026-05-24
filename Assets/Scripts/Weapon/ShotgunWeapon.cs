using UnityEngine;

public class ShotgunWeapon : WeaponBase
{
    [Header("Hitscan Cone")]
    public LayerMask enemyLayers;
    public float maxRange = 15f;
    [Min(1)] public int pelletCount = 8;
    public float coneAngle = 10f;

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public ParticleSystem hitEffect;

    protected override void TryFire()
    {
        Vector3 origin = transform.position;
        Vector3 baseDirection = transform.forward;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 dir = ApplyConeSpread(baseDirection);
            if (Physics.Raycast(origin, dir, out RaycastHit hit, maxRange, enemyLayers))
                HandlePelletHit(hit);
        }

        ApplyHeat(data.heatPerShot);
        SpawnMuzzleFlash();

        if (playerMotor != null)
            playerMotor.AddRecoil(data.recoilStrength, data.recoilDuration);
    }

    protected override void ExecuteSkill()
    {
        // Reserved for future "slug" alt-fire — single high-damage pellet
    }

    private void HandlePelletHit(RaycastHit hit)
    {
        if (!hit.collider.TryGetComponent<EnemyBase>(out var enemy))
        {
            enemy = hit.collider.GetComponentInParent<EnemyBase>();
            if (enemy == null) return;
        }

        float damage = data.baseDamage;
        if (!isOverheated && currentHeat >= data.optimalZoneStart && currentHeat <= data.optimalZoneEnd)
            damage *= data.optimalHeatMultiplier;

        enemy.TakeDamage(damage);

        if (hitEffect != null)
        {
            var fx = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
            fx.Play();
            Destroy(fx.gameObject, 1f);
        }
    }

    private Vector3 ApplyConeSpread(Vector3 forward)
    {
        float yaw = Random.Range(-coneAngle, coneAngle);
        float pitch = Random.Range(-coneAngle, coneAngle);
        return Quaternion.Euler(pitch, yaw, 0f) * forward;
    }

    private void SpawnMuzzleFlash()
    {
        if (muzzleFlashPrefab == null) return;
        var flash = Instantiate(muzzleFlashPrefab, transform.position, transform.rotation);
        Destroy(flash, 0.1f);
    }

    private void ApplyHeat(float amount)
    {
        currentHeat = Mathf.Clamp(currentHeat + amount, 0f, data.overheatThreshold);
        if (!isOverheated && currentHeat >= data.overheatThreshold)
            isOverheated = true;
    }
}
