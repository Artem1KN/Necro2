using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// Drives a ragdoll on enemy death.
///
/// Coexists with AI movement: the **root Rigidbody** (used by AI_Soldier /
/// AI_Zombie for MovePosition) is never touched. Only the *child* bone
/// Rigidbody's created by the Ragdoll Wizard are kept kinematic until death
/// and switched to dynamic when the enemy dies.
///
/// Setup:
/// 1. On the enemy prefab, run "GameObject > 3D Object > Ragdoll..." wizard
///    to create bone Rigidbody/Collider pairs on the skeleton.
/// 2. Add this component on the root enemy object — it auto-discovers all
///    *child* Rigidbody's at Awake and keeps them kinematic until death.
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

    [Tooltip("Disable bone colliders while the enemy is alive so they don't double-hit with the body collider. Re-enabled on death.")]
    public bool disableBoneCollidersWhileAlive = true;

    [Header("References (auto if empty)")]
    [Tooltip("Behaviours that should be disabled the moment the ragdoll starts (AI scripts, etc).")]
    public List<Behaviour> disableOnDeath = new();

    [Tooltip("Optional Animator that should be disabled to release bone control.")]
    public Animator animator;

    [Tooltip("Optional NavMeshAgent that should be disabled.")]
    public NavMeshAgent navAgent;

    private readonly List<Rigidbody> boneBodies = new();
    private readonly List<Collider> boneColliders = new();
    private Rigidbody rootRigidbody;
    private Collider rootCollider;
    private bool triggered;

    private void Awake()
    {
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (navAgent == null) navAgent = GetComponent<NavMeshAgent>();

        rootRigidbody = GetComponent<Rigidbody>();
        rootCollider = GetComponent<Collider>();

        // Collect every Rigidbody under us, then drop the root one so AI movement keeps working.
        var allBodies = new List<Rigidbody>();
        GetComponentsInChildren(true, allBodies);
        foreach (var rb in allBodies)
        {
            if (rb == rootRigidbody) continue;
            boneBodies.Add(rb);
            rb.isKinematic = true;
            // Bones must not respond to gravity until ragdoll triggers.
            rb.useGravity = false;
        }

        // Bone colliders are optional — leave them enabled if the user wants per-bone hitboxes.
        if (disableBoneCollidersWhileAlive)
        {
            var allCols = new List<Collider>();
            GetComponentsInChildren(true, allCols);
            foreach (var col in allCols)
            {
                if (col == rootCollider) continue;
                boneColliders.Add(col);
                col.enabled = false;
            }
        }
    }

    /// Activates physics on bones, disables AI / Animator / NavMeshAgent / root collider / root Rigidbody.
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

        // Freeze the root Rigidbody so AI scripts (now disabled) don't keep pushing the body around.
        if (rootRigidbody != null)
        {
            rootRigidbody.linearVelocity = Vector3.zero;
            rootRigidbody.angularVelocity = Vector3.zero;
            rootRigidbody.isKinematic = true;
        }

        // Bring bone colliders back to life so the ragdoll has solid geometry.
        if (disableBoneCollidersWhileAlive)
        {
            foreach (var col in boneColliders)
                col.enabled = true;
        }

        Vector3 impulse = hitDirection.sqrMagnitude > 0.001f
            ? hitDirection.normalized * deathImpulse
            : Vector3.up * deathImpulse;

        foreach (var rb in boneBodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
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
