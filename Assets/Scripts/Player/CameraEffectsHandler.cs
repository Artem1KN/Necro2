using UnityEngine;
using Unity.Cinemachine;

public class CameraEffectsHandler : MonoBehaviour
{
    [Header("FOV Settings")]
    [SerializeField] private float baseFOV = 60f;      // Обычный угол обзора
    [SerializeField] private float maxFOV = 80f;       // Угол при рывке/стене
    [SerializeField] private float fovTransitionSpeed = 10f; // Скорость изменения FOV

    [Header("References")]
    [SerializeField] private CinemachineCamera _cinemach;

    public float BaseFOV => baseFOV;
    public float MaxFOV => maxFOV;
    public float FovTransitionSpeed => fovTransitionSpeed;

    private float _currentFOV;

    void Start()
    {
        if (_cinemach != null)
        {
            _currentFOV = baseFOV;
            _cinemach.Lens.FieldOfView = baseFOV;
        }
    }

    public void HandleFOV(bool isWallRunning, float dashVelocity)
    {
        if (_cinemach == null) return;

        // Определяем целевой FOV в зависимости от состояния
        float targetFOV = baseFOV;

        // Если мы в рывке (dashVelocity еще высокий) или бежим по стене
        if (dashVelocity > 1f || isWallRunning)
        {
            targetFOV = maxFOV;
        }

        // Плавно переходим от текущего FOV к целевому
        _cinemach.Lens.FieldOfView = Mathf.Lerp(_cinemach.Lens.FieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }
}