using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(EnemyBase))]
public class AI_Zombie : MonoBehaviour
{
    [Header("Chase")]
    public float moveSpeed = 4f;

    [Header("Attack")]
    public float attackRange = 2.5f;
    public float damagePerHit = 50f;
    public float attackCooldown = 1f;
    public Vector3 attackOffset = new Vector3(0, 0, 1.2f);

    [Header("References")]
    public LayerMask playerLayer = ~0;

    [Header("Audio")]
    public AudioClip attackSfx;

    private enum State { Chase, Attack, Dead }

    private EnemyBase enemyBase;
    private Transform targetTransform;
    private Rigidbody rb;
    private State currentState = State.Chase;
    private float lastAttackTime = -Mathf.Infinity;
    private Vector3 chaseDirection;
    private Animator anim; // Добавляем переменную для аниматора

    private void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        anim = GetComponent<Animator>(); // Инициализируем аниматор

        if (enemyBase != null)
        {
            enemyBase.onDeath += () => currentState = State.Dead;
        }
    }

    private void Start()
    {
        targetTransform = FindFirstObjectByType<PlayerMotor>()?.transform
            ?? FindFirstObjectByType<TestPlayerController>()?.transform
            ?? GameObject.FindGameObjectWithTag("Player")?.transform;

        if (targetTransform == null)
            Debug.LogWarning("[AI_Zombie] Player not found. Standing idle.", this);
    }

    private void Update()
    {
        if (targetTransform == null || currentState == State.Dead) return;

        Vector3 toPlayer = targetTransform.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;
        chaseDirection = distance > 0.01f ? toPlayer / distance : Vector3.zero;

        currentState = distance <= attackRange ? State.Attack : State.Chase;
        // --- НОВАЯ ЛОГИКА ДЛЯ АНИМАЦИИ ПЕРЕМЕЩЕНИЯ ---
        bool isMoving = (currentState == State.Chase);
        anim.SetBool("isMoving", isMoving);

        if (currentState == State.Attack && Time.time - lastAttackTime >= attackCooldown)
            Attack();

        if (chaseDirection.sqrMagnitude > 0.001f)
        {
            var targetRot = Quaternion.LookRotation(chaseDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    private void FixedUpdate()
    {
        if (targetTransform == null || currentState != State.Chase) return;
        if (chaseDirection.sqrMagnitude < 0.001f) return;

        Vector3 step = chaseDirection * (moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + step);
    }

    private void Attack()
    {
        if (targetTransform == null) return;

        int targetLayer = targetTransform.gameObject.layer;
        if ((playerLayer.value & (1 << targetLayer)) == 0) return;

        lastAttackTime = Time.time;

        // --- НОВАЯ ЛОГИКА ДЛЯ АНИМАЦИИ АТАКИ ---
        anim.SetBool("isAttacking", true); 
        StartCoroutine(ResetAttackAnimation());

        var damagable = targetTransform.GetComponent<IDamagable>()
            ?? targetTransform.GetComponentInParent<IDamagable>();
        if (damagable != null)
            damagable.TakeDamage(damagePerHit);

        if (attackSfx != null)
            AudioManager.EnsureExists().PlaySfxAt(attackSfx, transform.position);
    }

    // --- НОВАЯ ЛОГИКА: Сброс анимации атаки ---
    private System.Collections.IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Подберите время под вашу анимацию зомби
        anim.SetBool("isAttacking", false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Vector3 worldCenter = transform.TransformPoint(attackOffset);
        Gizmos.DrawWireSphere(worldCenter, attackRange);
    }
#endif
}
