using UnityEngine;

public class HitscanWeapon : WeaponBase
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private GameObject impactEffect;

    public override void Attack()
    {
        if (IsOverheated) return;

        // Check attack cooldown
        if (Time.time < lastAttackTime + Data.attackCooldown) return;

        lastAttackTime = Time.time;
        PerformHitscan();
        AddHeat(Data.heatPerShot);

        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }
    }

    private void PerformHitscan()
    {
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, Data.attackRange))
        {
            // Handle Impact Visuals
            if (impactEffect != null)
            {
                Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            }

            // Handle Damage (Placeholder for actual damage system)
            Debug.Log($"Hit: {hit.collider.name} for {CalculateDamage()} damage");
        }
    }

    private float CalculateDamage()
    {
        float multiplier = Data.damageMultiplierCurve.Evaluate(CurrentHeatPercent);
        return Data.baseDamage * multiplier;
    }
}