using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GrenadeProjectile : MonoBehaviour
{
    [Header("Throw")]
    public float launchForce = 18f;
    public float upwardBias = 0.35f;
    public float fuseSeconds = 2.5f;

    [Header("Damage")]
    public float damage = 60f;
    public float explosionRadius = 4f;
    public LayerMask explosionLayers = ~0;
    public AnimationCurve falloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("FX")]
    public GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private float fuseTimer;
    private bool detonated;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(float damageValue, float radius)
    {
        damage = damageValue;
        explosionRadius = radius;

        Vector3 dir = (transform.forward + Vector3.up * upwardBias).normalized;
        rb.linearVelocity = dir * launchForce;
        rb.angularVelocity = Random.insideUnitSphere * 6f;

        fuseTimer = fuseSeconds;
    }

    private void Update()
    {
        if (detonated) return;
        fuseTimer -= Time.deltaTime;
        if (fuseTimer <= 0f) Detonate();
    }

    private void Detonate()
    {
        if (detonated) return;
        detonated = true;

        var hits = Physics.OverlapSphere(transform.position, explosionRadius, explosionLayers);
        var visited = new HashSet<int>();

        foreach (var col in hits)
        {
            int rootId = col.transform.root.GetInstanceID();
            if (!visited.Add(rootId)) continue;

            float distance = Vector3.Distance(transform.position, col.ClosestPoint(transform.position));
            float damageScale = falloff.Evaluate(Mathf.Clamp01(distance / explosionRadius));
            float finalDamage = damage * damageScale;

            if (col.TryGetComponent<EnemyBase>(out var enemy))
                enemy.TakeDamage(finalDamage);
        }

        if (explosionEffectPrefab != null)
        {
            var fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
