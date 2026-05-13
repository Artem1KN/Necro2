using System;
using UnityEngine;

/// <summary>
/// Простой AI-зомби с двумя состояниями:
/// 1. Chase — движение к игроку.
/// 2. Attack — остановка и атака при попадании в радиус attackRange.
/// Не использует NavMesh. Работает только в плоскости XZ.
/// Всегда стремится к игроку, пока не находится в зоне атаки (attackRange).
/// Атака проверяется в Update по дистанции и кулдауну — без зависимости от физических событий.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(EnemyBase))]
public class AI_Zombie : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 4f;

    [Header("Attack Settings")]
    public float attackRange = 2.5f;
    public float damagePerHit = 50f;
    public float attackCooldown = 1f; // Секунды между атаками

    [Tooltip("Смещение сферы атаки относительно центра модели (например, вперед)")]
    public Vector3 attackOffset = new Vector3(0, 0, 1.2f);

    [Header("References & Layers")]
    [Tooltip("Слой игрока (по умолчанию 'Player')")]
    public LayerMask playerLayer = 1 << 0;

    private enum State
    {
        Chase,
        Attack
    }

    // Компоненты
    private EnemyBase enemyBase;
    private Transform targetTransform;
    private Rigidbody rb;

    // Состояние
    private State currentState = State.Chase;
    private float lastAttackTime = -Mathf.Infinity;

    void Awake()
    {
        enemyBase = GetComponent<EnemyBase>();
        rb = GetComponent<Rigidbody>();

        // Создаём сферический триггер только для визуализации (Gizmos) или будущих нужд.
        // Он больше не участвует в логике атаки — проверка через дистанцию в Update.
        var sphereCollider = gameObject.AddComponent<SphereCollider>();
        sphereCollider.radius = attackRange;
        sphereCollider.isTrigger = true;
        sphereCollider.center = attackOffset;
    }

    void Start()
    {
        targetTransform = FindFirstObjectByType<PlayerMotor>()?.transform ?? GameObject.FindGameObjectWithTag("Player")?.transform;

        if (targetTransform == null)
        {
            Debug.LogWarning("[Zombie] Player not found! AI will stand idle.");
        }

        //rb.useGravity = false;
    }

    void Update()
    {
        if (targetTransform == null) return;

        // Расчёт расстояния до игрока
        float distanceToPlayer = Vector3.Distance(transform.position, targetTransform.position);

        // Определение состояния: атака — если в радиусе атаки
        currentState = (distanceToPlayer <= attackRange) ? State.Attack : State.Chase;

        switch (currentState)
        {
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                // Атакуем, если прошёл кулдаун и мы в зоне атаки
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack();
                }
                break;
        }

        // Поворот модели к игроку всегда при наличии цели
        if (targetTransform != null && currentState == State.Chase)
        {
            Vector3 direction = targetTransform.position - transform.position;
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    /// <summary>
    /// Движение к игроку.
    /// </summary>
    private void Chase()
    {
        if (targetTransform == null) return;

        Vector3 direction = targetTransform.position - transform.position;
        direction = Vector3.ProjectOnPlane(direction, Vector3.up); // только XZ
        direction.Normalize();

        rb.MovePosition(transform.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
    }

    /// <summary>
    /// Реализация атаки: наносит урон игроку.
    /// </summary>
    private void Attack()
    {
        // Проверка слоя игрока (доп. защита)
        int playerLayerValue = playerLayer.value;
        int targetLayer = targetTransform.gameObject.layer;

        if ((playerLayerValue & (1 << targetLayer)) == 0) return;

        lastAttackTime = Time.time;

        var playerDamagable = targetTransform.GetComponent<IDamagable>();
        if (playerDamagable != null)
        {
            float remainingHP = playerDamagable.TakeDamage(damagePerHit);
            Debug.Log($"[Zombie] Hit player! HP: {remainingHP}/{playerDamagable.MaxHP}");
        }
        else
        {
            Debug.LogWarning("[Zombie] Player doesn't implement IDamagable!");
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Визуализация зоны атаки (для отладки)
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Vector3 worldCenter = transform.TransformPoint(attackOffset);
        Gizmos.DrawWireSphere(worldCenter, attackRange);

        // Визуализация направления взгляда
        if (targetTransform != null)
        {
            Vector3 dir = targetTransform.position - transform.position;
            dir = Vector3.ProjectOnPlane(dir, Vector3.up);
            dir.Normalize();

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, 0.2f);
        }
    }
#endif
}
