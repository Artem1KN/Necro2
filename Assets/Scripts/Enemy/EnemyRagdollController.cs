using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// Drives a ragdoll on enemy death.
///
/// Setup:
/// 1. On the enemy prefab, run "GameObject > 3D Object > Ragdoll..." wizard
///    to create bone Rigidbody/Collider pairs.
/// 2. Add this component on the root enemy object — it auto-discovers all
///    child Rigidbody's at Start and keeps them kinematic until death.
/// 3. EnemyBase.Die() calls TriggerRagdoll() if this component is present,
///    skipping its immediate Destroy. Ragdoll despawns after despawnDelay.
[DisallowMultipleComponent]
public class EnemyRagdollController : MonoBehaviour
{
    [Header("Tuning")]
    [Tooltip("Seconds after death before the body is destroyed.")]
    public float despawnDelay = 6f;

    [Tooltip("Optional final impulse applied to all bones, in world units/s.")]
    public float deathImpulse = 1.5f;

    [Tooltip("If true, fade out via simple scale tween before destroy.")]
    public bool shrinkBeforeDestroy = true;

    [Header("References (auto if empty)")]
    [Tooltip("Behaviours that should be disabled the moment the ragdoll starts.")]
    public List<Behaviour> disableOnDeath = new();

    [Tooltip("Optional Animator that should be disabled to release bone control.")]
    public Animator animator;

    [Tooltip("Optional NavMeshAgent that should be disabled.")]
    public NavMeshAgent navAgent;

    private readonly List<Rigidbody> boneBodies = new();
    private readonly List<Collider> boneColliders = new();
    private Collider rootCollider;
    private bool triggered;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();
        rootCollider = GetComponent<Collider>();

        GetComponentsInChildren(true, boneBodies);
        foreach (var rb in boneBodies)
        {
            rb.isKinematic = true;
        }

        GetComponentsInChildren(true, boneColliders);
        foreach (var col in boneColliders)
        {
            // The main hit-detection collider stays enabled. Bone colliders start disabled.
            if (col == rootCollider) continue;
            col.enabled = false;
        }
    }

    /// Activates physics on bones, disables AI / Animator / NavMeshAgent / root collider.
    /// Schedules destroy after despawnDelay.
    public void TriggerRagdoll(Vector3 hitDirection)
    {
        if (triggered) return;
        triggered = true;

        if (animator != null) animator.enabled = false;
        if (navAgent != null) navAgent.enabled = false;
        if (rootCollider != null) rootCollider.enabled = false;
        foreach (var b in disableOnDeath)
            if (b != null) b.enabled = false;

        foreach (var col in boneColliders)
        {
            if (col == rootCollider) continue;
            col.enabled = true;
        }

        Vector3 impulse = hitDirection.sqrMagnitude > 0.001f
            ? hitDirection.normalized * deathImpulse
            : Vector3.up * deathImpulse;

        foreach (var rb in boneBodies)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(impulse, ForceMode.VelocityChange);
        }

        if (shrinkBeforeDestroy)
            StartCoroutine(ShrinkAndDestroy());
        else
            Destroy(gameObject, despawnDelay);
    }

    private System.Collections.IEnumerator ShrinkAndDestroy()
    {
        float t = 0f;
        float shrinkStart = despawnDelay - 1.0f;
        Vector3 baseScale = transform.localScale;
        while (t < despawnDelay)
        {
            t += Time.deltaTime;
            if (t > shrinkStart)
            {
                float k = Mathf.Clamp01((despawnDelay - t) / 1.0f);
                transform.localScale = baseScale * k;
            }
            yield return null;
        }
        Destroy(gameObject);
    }
}
