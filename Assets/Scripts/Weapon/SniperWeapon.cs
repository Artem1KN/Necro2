using System;
using UnityEngine;

public class SniperWeapon : WeaponBase
{
    [Header("Hitscan")]
    public LayerMask enemyLayers;
    public float maxRange = 100f;

    [Tooltip("Max enemies pierced by a single shot.")]
    [Range(1, 5)] public int maxPenetrations = 2;

    [Tooltip("Damage multiplier applied to each subsequent target (1 = full, 0.5 = half on each next).")]
    [Range(0.1f, 1f)] public float penetrationFalloff = 0.7f;

    [Header("Effects")]
    public GameObject muzzleFlashPrefab;
    public GameObject hitEffectPrefab;
    public LineRenderer beamRenderer;
    public float beamDuration = 0.05f;

    private float beamTimer;

    protected override void TryFire()
    {
        var cam = Camera.main;
        Vector3 origin = cam != null ? cam.transform.position : transform.position;
        Vector3 direction = cam != null ? cam.transform.forward : transform.forward;

        var hits = Physics.RaycastAll(origin, direction, maxRange, enemyLayers);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        int hitsApplied = 0;
        float damage = data.baseDamage;

        foreach (var hit in hits)
        {
            if (hitsApplied >= maxPenetrations) break;

            EnemyBase enemy = hit.collider.GetComponent<EnemyBase>()
                ?? hit.collider.GetComponentInParent<EnemyBase>();
            if (enemy == null) continue;

            enemy.TakeDamage(damage);
            SpawnHitEffect(hit);

            damage *= penetrationFalloff;
            hitsApplied++;
        }

        ApplyHeat(data.heatPerShot);
        SpawnMuzzleFlash();
        FireBeam(origin, origin + direction * maxRange);

        AddRecoilFromData();
        PlayFireSfx();
    }

    protected override void ExecuteSkill()
    {
        // Reserved for scope / zoom alt-fire
    }

    private void Update()
    {
        if (beamTimer <= 0f || beamRenderer == null) return;

        beamTimer -= Time.deltaTime;
        if (beamTimer <= 0f) beamRenderer.enabled = false;
    }

    private void FireBeam(Vector3 from, Vector3 to)
    {
        if (beamRenderer == null) return;
        beamRenderer.enabled = true;
        beamRenderer.positionCount = 2;
        beamRenderer.SetPosition(0, from);
        beamRenderer.SetPosition(1, to);
        beamTimer = beamDuration;
    }

    private void SpawnHitEffect(RaycastHit hit)
    {
        if (hitEffectPrefab == null) return;
        var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(fx, 1f);
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
