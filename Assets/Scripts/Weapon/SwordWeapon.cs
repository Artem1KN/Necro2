using System.Collections.Generic;
using UnityEngine;

public class SwordWeapon : WeaponBase
{
    [Header("Melee Attack")]
    public LayerMask enemyLayers;
    public float attackRange = 2f;
    public float attackRadius = 1f;
    public Transform attackPoint;

    [Header("Parry / Block")]
    [Tooltip("Time window after RMB during which projectiles are deflected.")]
    public float parryWindowSeconds = 0.25f;

    [Tooltip("Radius around the player searched for projectiles during parry.")]
    public float parryRadius = 3f;

    [Tooltip("Damage multiplier applied to a deflected projectile.")]
    public float deflectDamageMultiplier = 2f;

    [Tooltip("Layers searched for deflectable projectiles. Set to your projectile layer.")]
    public LayerMask deflectableLayers = ~0;

    private float parryTimer;
    private bool parrySucceededThisWindow;

    public bool IsParryActive => parryTimer > 0f && !parrySucceededThisWindow;

    protected override void TryFire()
    {
        PerformAttack(data.baseDamage);
    }

    private void PerformAttack(float damage)
    {
        if (attackPoint == null)
        {
            Debug.LogError("[Sword] Attack point not assigned!", this);
            return;
        }

        Collider[] hits = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);
        var enemiesHit = new HashSet<EnemyBase>();

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyBase>(out var enemy) && enemiesHit.Add(enemy))
                enemy.TakeDamage(damage);
        }

        if (enemiesHit.Count > 0)
            ApplyHeat(data.heatPerShot);
    }

    protected override void ExecuteSkill()
    {
        parryTimer = parryWindowSeconds;
        parrySucceededThisWindow = false;
    }

    private void Update()
    {
        if (parryTimer <= 0f) return;

        parryTimer -= Time.deltaTime;
        if (!parrySucceededThisWindow)
            TryParryProjectiles();
    }

    private void TryParryProjectiles()
    {
        var origin = playerMotor != null ? playerMotor.transform.position : transform.position;
        var forward = GetAimForward();

        var hits = Physics.OverlapSphere(origin, parryRadius, deflectableLayers);
        foreach (var hit in hits)
        {
            if (!hit.TryGetComponent<IDeflectable>(out var deflectable))
            {
                deflectable = hit.GetComponentInParent<IDeflectable>();
                if (deflectable == null) continue;
            }

            var ownerTransform = playerMotor != null ? playerMotor.transform : transform;
            if (deflectable.TryDeflect(forward, ownerTransform, deflectDamageMultiplier))
            {
                parrySucceededThisWindow = true;
                if (data.skillUsesHeat && !isOverheated)
                    ApplyHeat(data.heatPerSkill);
                break;
            }
        }
    }

    private Vector3 GetAimForward()
    {
        var cam = Camera.main;
        if (cam != null) return cam.transform.forward;
        if (playerMotor != null) return playerMotor.transform.forward;
        return transform.forward;
    }

    private void ApplyHeat(float amount)
    {
        currentHeat = Mathf.Clamp(currentHeat + amount, 0f, data.overheatThreshold);
        if (!isOverheated && currentHeat >= data.overheatThreshold)
            isOverheated = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, parryRadius);
    }
}
