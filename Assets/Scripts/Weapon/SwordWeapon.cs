using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening; // Обязательно добавь это

public class SwordWeapon : WeaponBase
{
    [Header("Animation Settings")]
    [SerializeField] private Transform weaponTransform; // Ссылка на визуальную часть меча
    [SerializeField] private float attackDuration = 0.2f;
    [SerializeField] private float attackRotationAngle = 45f;
    [SerializeField] private float blockRotationAngle = -30f;
    [SerializeField] private float recoverRotationAngle = 0f;

    // Чтобы не плодить лишние переменные, будем использовать локальный поворот
    private Vector3 _initialRotation;

    protected override void ShootLogic(float damage)
    {
        Debug.Log($"Sword Attack! Damage: {damage}");
        
        // Анимация взмаха: быстрый поворот вперед и возврат
        // Используем Sequence, чтобы анимация была последовательной
        Sequence attackSeq = DOTween.Sequence();

        attackSeq.Append(weaponTransform.DOLocalRotate(new Vector3(attackRotationAngle, 0, 0), attackDuration * 0.5f)
            .SetEase(Ease.OutQuad)) // Быстрый взмах
            .Append(weaponTransform.DOLocalRotate(new Vector3(recoverRotationAngle, 0, 0), attackDuration * 0.5f)
            .SetEase(Ease.InQuad)); // Плавный возврат
    }

    protected override void ExecuteSkill()
    {
        Debug.Log("Sword Blocking...");
        
        // Анимация блока: смещение меча чуть вперед или поворот в защитную позицию
        weaponTransform.DOKill(); // Останавливаем предыдущие анимации, чтобы не было конфликтов
        weaponTransform.DOLocalRotate(new Vector3(blockRotationAngle, 0, 0), attackDuration)
            .SetEase(Ease.OutBack);
    }

    public override void OnAttack(InputValue value)
    {
        Debug.Log("Sword OnAttack...");
        if (value.isPressed) TryFire();
    }

    // Вспомогательный метод для сброса позиции (вызывать, если меч "застрял" в анимации)
    public void ResetWeaponPosition()
    {
        weaponTransform.DOKill();
        weaponTransform.localRotation = Quaternion.identity;
    }
}
