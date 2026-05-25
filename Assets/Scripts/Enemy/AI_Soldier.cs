using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody), typeof(EnemyBase))]
public class AI_Soldier : MonoBehaviour
{
    [Header("Chase")]
    public float moveSpeed = 5f;

    [Header("Ranged Attack")]
    [FormerlySerializedAs("attackRange")]
    public float fireDistance = 15f;
    public float damagePerHit = 20f;
    public float attackCooldown = 2f;
    public float projectileSpeed = 20f;
    public GameObject sphereProjectilePrefab;

    [Header("Combat Tuning")]
    [Tooltip("Aim accuracy. 1 = perfect shot at the player. 0 = max spread.")]
    [Range(0f, 1f)] public float accuracy = 0.75f;

    [Tooltip("Aggression. Boosts effective accuracy and reduces fire interval.")]
    [Range(0f, 1f)] public float aggressiveness = 0.5f;

    [Tooltip("Max angular spread (degrees) when accuracy is 0.")]
    public float maxSpreadDegrees = 12f;

    [Header("References")]
    public LayerMask playerLayer = 1 << 0;

    private enum State { Chase, Attack }

    private EnemyBase enemyBase;
    private Transform targetTransform;
    private Rigidbody rb;

    private State currentState = State.Chase;
    private float lastAttackTime = -Mathf.Infinity;

    private float EffectiveAccuracy => Mathf.Clamp01(accuracy + aggressiveness * 0.3f);
    private float EffectiveCooldown => attackCooldown * Mathf.Clamp(1f - aggressiveness * 0.5f, 0.1f, 1f);

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        targetTransform = FindFirstObjectByType<PlayerMotor>()?.transform
            ?? GameObject.FindGameObjectWithTag("Player")?.transform;

        if (targetTransform == null)
            Debug.LogWarning("[AI_Soldier] Player not found. Standing idle.", this);
    }

    private void Update()
    {
        if (targetTransform == null) return;

        float distance = Vector3.Distance(transform.position, targetTransform.position);
        currentState = distance <= fireDistance ? State.Attack : State.Chase;

        switch (currentState)
        {
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                if (Time.time - lastAttackTime >= EffectiveCooldown)
                    Attack();
                break;
        }

        FaceTarget();
    }

    private void FaceTarget()
    {
        Vector3 direction = targetTransform.position - transform.position;
        direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        if (direction.sqrMagnitude < 0.0001f) return;

        var targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
    }

    private void Chase()
    {
        Vector3 step = transform.forward * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(transform.position + step);
    }

    private void Attack()
    {
        lastAttackTime = Time.time;

        if (sphereProjectilePrefab == null)
        {
            Debug.LogWarning("[AI_Soldier] SphereProjectile prefab not assigned.", this);
            return;
        }

        Vector3 aim = (targetTransform.position - transform.position).normalized;
        aim = ApplySpread(aim);

        var spawnPos = transform.position + transform.forward;
        var projectileObj = Instantiate(sphereProjectilePrefab, spawnPos, Quaternion.LookRotation(aim));

        if (projectileObj.TryGetComponent<SphereProjectile>(out var projectile))
            projectile.Initialize(damagePerHit, transform, targetTransform);
    }

    private Vector3 ApplySpread(Vector3 direction)
    {
        float spreadDegrees = Mathf.Lerp(maxSpreadDegrees, 0f, EffectiveAccuracy);
        if (spreadDegrees <= 0f) return direction;

        float yaw = Random.Range(-spreadDegrees, spreadDegrees);
        float pitch = Random.Range(-spreadDegrees, spreadDegrees);
        return Quaternion.Euler(pitch, yaw, 0f) * direction;
    }
}
