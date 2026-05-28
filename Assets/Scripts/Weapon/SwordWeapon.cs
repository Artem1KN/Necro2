using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // Не забудьте добавить DOTween

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

    [Header("Sword Animation")]
    [SerializeField] private Transform swordTransform;     // модель меча в руках
    [SerializeField] private float attackDuration = 0.25f;
    [SerializeField] private float blockDuration  = 0.15f;

    [Header("Attack Pose")]
    [SerializeField] private Vector3 attackPosOffset = new Vector3(0.1f, 0.05f, 0.3f);
    [SerializeField] private Vector3 attackRotEuler  = new Vector3(-60f, 30f, 20f);

    [Header("Block Pose")]
    [SerializeField] private Vector3 blockPosOffset = new Vector3(0f, 0.1f, 0.25f);
    [SerializeField] private Vector3 blockRotEuler  = new Vector3(0f, -90f, 60f);

    private Vector3 idlePos;
    private Quaternion idleRot;
    private bool isAttacking;
    private bool isBlocking;
    private Sequence currentAnim;

    private void Awake()
    {
        if (swordTransform != null)
        {
            idlePos = swordTransform.localPosition;
            idleRot = swordTransform.localRotation;
        }
    }


    protected override void TryFire()
    {
        PerformAttack(data.baseDamage);
        PlayFireSfx();
        PlayAttackAnimation();
    }

    private void PerformAttack(float damage)
    {
        // Fall back to camera forward → 1.5m so the sword still hits even if attackPoint was never wired up.
        Vector3 origin;
        if (attackPoint != null)
        {
            origin = attackPoint.position;
        }
        else
        {
            var cam = Camera.main;
            var holder = playerMotor != null ? playerMotor.transform : transform;
            origin = (cam != null ? cam.transform.position : holder.position)
                   + (cam != null ? cam.transform.forward : holder.forward) * 1.2f;
        }

        int mask = enemyLayers.value == 0 ? ~0 : enemyLayers.value;
        Collider[] hits = Physics.OverlapSphere(origin, attackRange, mask, QueryTriggerInteraction.Ignore);
        var enemiesHit = new HashSet<EnemyBase>();

        foreach (var hit in hits)
        {
            if (hit.transform.root.CompareTag("Player")) continue;
            var enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy != null && enemiesHit.Add(enemy))
            {
                enemy.TakeDamage(damage);
                Debug.Log($"[Sword] hit {enemy.name} for {damage}");
            }
        }

        if (enemiesHit.Count > 0)
            ApplyHeat(data.heatPerShot);
    }

    protected override void ExecuteSkill()
    {
        parryTimer = parryWindowSeconds;
        parrySucceededThisWindow = false;
        PlaySkillSfx();
        PlayBlockAnimation();
    }

    private void Update()
    {
        if (parryTimer <= 0f) return;

        parryTimer -= Time.deltaTime;
        if (!parrySucceededThisWindow)
            TryParryProjectiles();
            EndBlock();
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



            // короткий "флик" - визуальный отклик парирования
                Quaternion flickRot = idleRot * Quaternion.Euler(-30f, 0f, -45f);
                currentAnim = DOTween.Sequence()
                    .Append(swordTransform.DOLocalRotateQuaternion(flickRot, 0.08f).SetEase(Ease.OutQuad))
                    .Append(swordTransform.DOLocalRotateQuaternion(idleRot, 0.15f).SetEase(Ease.InOutSine))
                    .OnComplete(() => isBlocking = false);

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


    private void PlayAttackAnimation()
    {
        if (swordTransform == null || isAttacking) return;

        isAttacking = true;
        currentAnim?.Kill();

        Vector3 startPos = idlePos;
        Quaternion startRot = idleRot;
        Vector3 endPos = idlePos + attackPosOffset;
        Quaternion endRot = idleRot * Quaternion.Euler(attackRotEuler);

        currentAnim = DOTween.Sequence()
            .Append(swordTransform.DOLocalMove(endPos, attackDuration * 0.4f).SetEase(Ease.OutQuad))
            .Join(swordTransform.DOLocalRotateQuaternion(endRot, attackDuration * 0.4f).SetEase(Ease.OutQuad))
            .Append(swordTransform.DOLocalMove(startPos, attackDuration * 0.6f).SetEase(Ease.InOutSine))
            .Join(swordTransform.DOLocalRotateQuaternion(startRot, attackDuration * 0.6f).SetEase(Ease.InOutSine))
            .OnComplete(() => isAttacking = false);
    }

    private void PlayBlockAnimation()
    {
        if (swordTransform == null) return;

        isBlocking = true;
        currentAnim?.Kill();

        Quaternion blockRot = idleRot * Quaternion.Euler(blockRotEuler);

        currentAnim = DOTween.Sequence()
            .Append(swordTransform.DOLocalMove(idlePos + blockPosOffset, blockDuration).SetEase(Ease.OutBack))
            .Join(swordTransform.DOLocalRotateQuaternion(blockRot, blockDuration).SetEase(Ease.OutBack));
    }

    private void EndBlock()
    {
        if (swordTransform == null || !isBlocking) return;

        isBlocking = false;
        currentAnim?.Kill();

        currentAnim = DOTween.Sequence()
            .Append(swordTransform.DOLocalMove(idlePos, blockDuration).SetEase(Ease.InOutSine))
            .Join(swordTransform.DOLocalRotateQuaternion(idleRot, blockDuration).SetEase(Ease.InOutSine));
    }
}
