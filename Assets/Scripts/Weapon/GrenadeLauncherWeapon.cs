using UnityEngine;

public class GrenadeLauncherWeapon : WeaponBase
{
    [Header("Projectile")]
    public GameObject grenadePrefab;
    public Transform spawnPoint;

    [Header("Damage")]
    public float explosionRadius = 4f;

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;

    protected override void TryFire()
    {
        if (grenadePrefab == null || spawnPoint == null)
        {
            Debug.LogError("[GrenadeLauncher] grenadePrefab or spawnPoint is missing.", this);
            return;
        }

        var grenadeGo = Instantiate(grenadePrefab, spawnPoint.position, spawnPoint.rotation);
        if (grenadeGo.TryGetComponent<GrenadeProjectile>(out var grenade))
            grenade.Launch(data.baseDamage, explosionRadius);

        ApplyHeat(data.heatPerShot);
        SpawnMuzzleFlash();

        if (playerMotor != null)
            playerMotor.AddRecoil(data.recoilStrength, data.recoilDuration);
    }

    protected override void ExecuteSkill()
    {
        // Reserved: sticky grenade or cluster mode
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
