using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RocketProjectile : MonoBehaviour
{
    [Header("Flight")]
    public float speed = 25f;
    public float lifeSpan = 6f;

    [Header("Damage")]
    public float damage = 80f;
    public float explosionRadius = 5f;
    public LayerMask explosionLayers = ~0;
    public AnimationCurve falloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    [Header("Rocket Jump")]
    public float playerImpulse = 12f;
    public float selfDamageMultiplier = 0.25f;

    [Header("FX")]
    public GameObject explosionEffectPrefab;

    private Rigidbody rb;
    private Transform owner;
    private bool detonated;

    public bool HasDetonated => detonated;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Launch(Transform shooter, float damageValue, float radius)
    {
        owner = shooter;
        damage = damageValue;
        explosionRadius = radius;
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeSpan);
    }

    private void OnCollisionEnter(Collision _)
    {
        Detonate();
    }

    public void Detonate()
    {
        if (detonated) return;
        detonated = true;

        ApplyAreaDamage();
        SpawnExplosionEffect();
        Destroy(gameObject);
    }

    private void ApplyAreaDamage()
    {
        var hits = Physics.OverlapSphere(transform.position, explosionRadius, explosionLayers);
        var visited = new HashSet<int>();

        foreach (var col in hits)
        {
            int rootId = col.transform.root.GetInstanceID();
            if (!visited.Add(rootId)) continue;

            float distance = Vector3.Distance(transform.position, col.bounds.ClosestPoint(transform.position));
            float normalized = Mathf.Clamp01(distance / explosionRadius);
            float damageScale = falloff.Evaluate(normalized);
            float finalDamage = damage * damageScale;

            EnemyBase enemy = col.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(finalDamage);
                continue;
            }

            PlayerHealth player = col.GetComponent<PlayerHealth>();
            if (player == null) player = col.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(finalDamage * selfDamageMultiplier);
                ApplyRocketJump(col.transform);
            }
        }
    }

    private void ApplyRocketJump(Transform playerTransform)
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.up;
        Vector3 impulse = direction * playerImpulse;

        var motor = playerTransform.GetComponent<PlayerMotor>() ?? playerTransform.GetComponentInParent<PlayerMotor>();
        if (motor != null)
        {
            motor.ApplyExternalImpulse(impulse);
            return;
        }

        var test = playerTransform.GetComponent<TestPlayerController>() ?? playerTransform.GetComponentInParent<TestPlayerController>();
        if (test != null) test.ApplyExternalImpulse(impulse);
    }

    private void SpawnExplosionEffect()
    {
        if (explosionEffectPrefab == null) return;
        var fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        Destroy(fx, 3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
