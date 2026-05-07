using UnityEngine;

public class MeleeWeapon : WeaponBase
{
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float damageMultiplier = 1.5f;
    [SerializeField] private LayerMask enemyLayer;

    public override void Attack()
    {
        // Melee doesn't overheat as per GDD
        if (Time.time < lastAttackTime + Data.attackCooldown) return;

        lastAttackTime = Time.time;
        PerformMeleeAttack();
    }

    private void PerformMeleeAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        foreach (var hitCollider in hitColliders)
        {
            // In a real implementation, we'd call an IDamageable interface
            Debug.Log($"Melee Hit: {hitCollider.name} for {Data.baseDamage * damageMultiplier} damage");
        }
    }

    // Override to ensure melee never adds heat despite potential logic errors elsewhere
    public override void AddHeat(float amount)
    {
        // Melee is explicitly infinite/no heat
    }
}