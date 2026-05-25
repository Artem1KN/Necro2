using UnityEngine;

/// <summary>
/// Сферический снаряд, летящий по прямой.
/// При попадании в игрока наносит урон.
/// При успешном блоке/парировании: разворачивается и летит обратно, нанося урон стрелявшему врагу.
/// </summary>
public class SphereProjectile : MonoBehaviour, IDeflectable
{
    public bool TryDeflect(Vector3 newDirection, Transform newOwner, float damageMultiplier)
    {
        if (hasHit) return false;

        isReflected = true;
        shooterTransform = newOwner;
        damage *= Mathf.Max(1f, damageMultiplier);

        if (rb != null)
        {
            float boostedSpeed = speed * 1.4f;
            rb.linearVelocity = newDirection.normalized * boostedSpeed;
        }

        return true;
    }

    [Header("Settings")]
    public float damage = 20f;
    public float lifeSpan = 8f;
    public float speed = 20f;

    private Rigidbody rb;
    private Transform shooterTransform;
    private Transform targetTransform;
    private bool hasHit = false;
    private bool isReflected = false;

    public void Initialize(float dmg, Transform shooter, Transform target)
    {
        damage = dmg;
        shooterTransform = shooter;
        targetTransform = target;
        rb = GetComponent<Rigidbody>();

        if (rb != null)
        {
            // Летим к цели
            Vector3 direction = (target != null) ? (target.position - transform.position).normalized : transform.forward;
            rb.linearVelocity = direction * speed;
        }

        // Уничтожаем через время жизни
        Destroy(gameObject, lifeSpan);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        var playerDamagable = other.GetComponentInParent<IDamagable>();
        if (playerDamagable != null && targetTransform != null && other.gameObject == targetTransform.gameObject)
        {
            playerDamagable.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
            return;
        }

        if (isReflected && shooterTransform != null && other.gameObject == shooterTransform.gameObject)
        {
            var shooterDamagable = other.GetComponentInParent<IDamagable>();
            if (shooterDamagable != null) shooterDamagable.TakeDamage(damage);
            hasHit = true;
            Destroy(gameObject);
        }
    }

    private void HandlePlayerBlock(Collider playerCollider)
    {
        // Проверяем, есть ли у игрока компонент блокировки
        // Блокировка/парирование вызывается через PlayerMotor или SwordWeapon
        // Здесь просто отмечаем, что снаряд был заблокирован
        // Реальная логика блокировки находится в SwordWeapon.ExecuteSkill()
        // Этот метод вызывается из SphereProjectile, если игрок заблокировал
        // Но для простоты: если враг-солдат получит событие о блокировке, он отразит снаряд

        // Имитация: проверяем расстояние до стрелявшего
        if (shooterTransform != null)
        {
            float distanceToShooter = Vector3.Distance(transform.position, shooterTransform.position);
            if (distanceToShooter < 30f) // Если стрелявший всё ещё близко
            {
                // Отражаем снаряд
                ReflectProjectile();
            }
        }
    }

    private void ReflectProjectile()
    {
        isReflected = true;

        if (rb != null && shooterTransform != null)
        {
            Vector3 direction = (shooterTransform.position - transform.position).normalized;
            rb.linearVelocity = direction * speed * 1.2f; // Чуть быстрее при отражении
        }

        Debug.Log("[SphereProjectile] Reflected back!");
    }

    private void OnDestroy()
    {
        // Cleanup
    }
}
