using System;
using UnityEngine;

/// <summary>
/// Soldier AI с двумя состояниями:
/// 1. Chase — преследование игрока.
/// 2. Attack — выстрел сферой по прямой к игроку.
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(EnemyBase))]
public class AI_Soldier : MonoBehaviour
{
    [Header("Chase Settings")]
    public float moveSpeed = 5f;

    [Header("Attack Settings")]
    public float attackRange = 15f;
    public float damagePerHit = 20f;
    public float attackCooldown = 2f;
    public float projectileSpeed = 20f;
    public GameObject sphereProjectilePrefab;

    [Header("References & Layers")]
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
    }

    void Start()
    {
        targetTransform = FindFirstObjectByType<PlayerMotor>()?.transform ?? GameObject.FindGameObjectWithTag("Player")?.transform;

        if (targetTransform == null)
        {
            Debug.LogWarning("[Soldier] Player not found! AI will stand idle.");
        }
    }

    void Update()
    {
        if (targetTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, targetTransform.position);

        currentState = (distanceToPlayer <= attackRange) ? State.Attack : State.Chase;

        switch (currentState)
        {
            case State.Chase:
                Chase();
                break;
            case State.Attack:
                if (Time.time - lastAttackTime >= attackCooldown)
                {
                    Attack();
                }
                break;
        }

        // Поворот к игроку
        if (targetTransform != null)
        {
            Vector3 direction = targetTransform.position - transform.position;
            direction = Vector3.ProjectOnPlane(direction, Vector3.up);
            direction.Normalize();
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void Chase()
    {
        if (targetTransform == null) return;

        Vector3 direction = targetTransform.position - transform.position;
        direction = Vector3.ProjectOnPlane(direction, Vector3.up);
        direction.Normalize();

        rb.MovePosition(transform.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
    }

    private void Attack()
    {
        lastAttackTime = Time.time;

        if (sphereProjectilePrefab != null)
        {
            GameObject projectileObj = Instantiate(sphereProjectilePrefab, transform.position + transform.forward * 1f, Quaternion.LookRotation(targetTransform.position - transform.position));
            SphereProjectile projectile = projectileObj.GetComponent<SphereProjectile>();

            if (projectile != null)
            {
                projectile.Initialize(damagePerHit, transform, targetTransform);
            }
        }
        else
        {
            Debug.LogWarning("[Soldier] SphereProjectile prefab not assigned!");
        }
    }
}
