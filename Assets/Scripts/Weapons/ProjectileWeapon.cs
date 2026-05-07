using UnityEngine;

public class ProjectileWeapon : WeaponBase
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private ParticleSystem muzzleFlash;

    public override void Attack()
    {
        if (IsOverheated) return;

        if (Time.time < lastAttackTime + Data.attackCooldown) return;

        lastAttackTime = Time.time;
        FireProjectile();
        AddHeat(Data.heatPerShot);

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null) return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        // Assuming the projectile has a Rigidbody or similar to move forward
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.forward * 20f; // Default velocity, should ideally be in WeaponData
        }
    }
}