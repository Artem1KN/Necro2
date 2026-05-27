using UnityEngine;

public class RocketLauncherWeapon : WeaponBase
{
    [Header("Projectile")]
    public GameObject rocketPrefab;
    public Transform spawnPoint;

    [Header("Damage")]
    public float explosionRadius = 5f;

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;

    private RocketProjectile activeRocket;

    protected override void TryFire()
    {
        if (rocketPrefab == null || spawnPoint == null)
        {
            Debug.LogError("[RocketLauncher] rocketPrefab or spawnPoint is missing.", this);
            return;
        }

        var rocketGo = Instantiate(rocketPrefab, spawnPoint.position, spawnPoint.rotation);
        if (rocketGo.TryGetComponent<RocketProjectile>(out var rocket))
        {
            rocket.Launch(transform, data.baseDamage, explosionRadius);
            activeRocket = rocket;
        }

        ApplyHeat(data.heatPerShot);
        SpawnMuzzleFlash();

        AddRecoilFromData();
        PlayFireSfx();
    }

    protected override void ExecuteSkill()
    {
        if (activeRocket == null) return;
        if (activeRocket.HasDetonated)
        {
            activeRocket = null;
            return;
        }

        activeRocket.Detonate();
        activeRocket = null;
    }

    private void SpawnMuzzleFlash()
    {
        if (muzzleFlashPrefab == null || spawnPoint == null) return;
        var flash = Instantiate(muzzleFlashPrefab, spawnPoint.position, spawnPoint.rotation);
        Destroy(flash, 0.15f);
    }

    private void ApplyHeat(float amount)
    {
        currentHeat = Mathf.Clamp(currentHeat + amount, 0f, data.overheatThreshold);
        if (!isOverheated && currentHeat >= data.overheatThreshold)
            isOverheated = true;
    }
}
