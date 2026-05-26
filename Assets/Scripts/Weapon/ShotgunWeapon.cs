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
    public GameObject hitEffectPrefab;
    public GameObject tracerPrefab;
    public Transform muzzlePoint;

    protected override void TryFire()
    {
        var cam = Camera.main;
        Vector3 origin = cam != null ? cam.transform.position : transform.position;
        Vector3 baseDirection = cam != null ? cam.transform.forward : transform.forward;
        Vector3 tracerFrom = muzzlePoint != null ? muzzlePoint.position : transform.position;

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 dir = ApplyConeSpread(baseDirection);
            bool hit = WeaponRaycastUtil.RaycastSkippingPlayer(origin, dir, maxRange, enemyLayers, out RaycastHit info);
            Vector3 tracerTo = hit ? info.point : origin + dir * maxRange;
            SpawnTracer(tracerFrom, tracerTo);
            if (hit) HandlePelletHit(info);
        }

        ApplyHeat(data.heatPerShot);
        SpawnMuzzleFlash();
        AddRecoilFromData();
    }

    private void SpawnTracer(Vector3 from, Vector3 to)
    {
        if (tracerPrefab == null) return;
        var go = Instantiate(tracerPrefab, from, Quaternion.identity);
        if (go.TryGetComponent<BulletTracer>(out var tracer)) tracer.Setup(from, to);
        Destroy(go, 0.2f);
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

        if (hitEffectPrefab != null)
        {
            var fx = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(fx, 1f);
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
