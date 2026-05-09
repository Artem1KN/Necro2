using UnityEngine;
using DG.Tweening;

public class SwordWeapon : WeaponBase
{
    [Header("Animation Settings")]
    [SerializeField] private Transform weaponTransform;
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackRotationAngle = 45f;
    [SerializeField] private float blockRotationAngle = -30f;
    [SerializeField] private float recoverRotationAngle = 0f;
    private Vector3 _initialRotation;
    
    protected void ShootLogic(float damage)
    {
        Debug.Log($"[Sword] Attack! Damage: {damage} (from SwordWeapon.ShootLogic)");

        // Анимация взмаха: быстрый поворот вперед и возврат
        Sequence attackSeq = DOTween.Sequence();
        attackSeq.Append(weaponTransform.DOLocalRotate(new Vector3(attackRotationAngle, 0, 0), attackDuration * 0.5f)
            .SetEase(Ease.OutQuad)) // Быстрый взмах
            .Append(weaponTransform.DOLocalRotate(new Vector3(recoverRotationAngle, 0, 0), attackDuration * 0.5f)
            .SetEase(Ease.InQuad)); // Плавный возврат
    }

    // Добавлено: обработка TryFire для Sword (вызывается каждые fireRate приHoldAttack)
    protected override void TryFire()
    {
        Debug.Log("[Sword] TryFire called - triggering attack");

        // Если оружие не перегрето и можно стрелять
        if (!isOverheated && Time.time >= lastFireTime + data.fireRate)
        {
            lastFireTime = Time.time;
            ShootLogic(data.baseDamage); // <-- здесь происходит атака меча

            // Можно добавить проверку на блок: если сейчас в режиме block, не атаковать
            if (weaponTransform.localRotation.eulerAngles.x == blockRotationAngle)
            {
                Debug.LogWarning("[Sword] Cannot attack while blocking!");
                return;
            }
        }
        else if (isOverheated)
        {
            Debug.LogWarning("[Sword] Weapon is overheated!");
        }
    }

    protected override void ExecuteSkill()
    {
        Debug.Log("Sword Blocking...");
        // Анимация блока: смещение меча чуть вперед или поворот в защитную позицию
        weaponTransform.DOKill(); // Останавливаем предыдущие анимации, чтобы не было конфликтов
        weaponTransform.DOLocalRotate(new Vector3(blockRotationAngle, 0, 0), attackDuration)
            .SetEase(Ease.OutBack);
    }

    // Дополнительно: можно добавить дебаг при старте/окончании атаки
    private void HandleAttackStarted()
    {
        Debug.Log("[Sword] Attack started (HandleAttackStarted)");
    }

    private void HandleAttackEnded()
    {
        Debug.Log("[Sword] Attack ended (HandleAttackEnded)");
    }

    // Вспомогательный метод для сброса позиции (вызывать, если меч "застрял" в анимации)
    public void ResetWeaponPosition()
    {
        weaponTransform.DOKill();
        weaponTransform.localRotation = Quaternion.identity;
    }
}
