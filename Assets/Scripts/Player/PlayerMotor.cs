using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMotor : MonoBehaviour
{
    [Header("Movement Settings")]
    public float runSpeed = 5f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    public float currentSpeed;
    [Header("Dash Settings")]
    public float dashImpulse = 15f;
    private float _dashCooldown = 0f;
    private float _dashVelocity;
    [Header("Jump & Gravity")]
    public float jumpHeight = 2f;
    public float gravity = -19.62f;
    private float _verticalVelocity;
    private int _jumpsRemaining;
    public int maxJumps = 2;
    private bool _jumpPressedThisFrame = false; // Флаг для фиксации нажатия прыжка
    [Header("Wall Checkers")]
    public CollisionDetectorRaycast leftWallDetector;
    public CollisionDetectorRaycast rightWallDetector;
    [Header("Wall Run Settings")]
    public float wallRunSpeed = 7f; // Скорость при беге по стене
    public float wallJumpForce = 10f; // Сила отталкивания от стены
    public float wallRunGravityMultiplier = 0.2f; // Насколько слабой будет гравитция на стене
    private bool _isWallRunning = false;
    private Vector3 _wallNormal; // Нормаль стены, чтобы знать куда отталкиваться
    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cinemach;
    private Vector2 _move;
    private Vector3 _lastMoveDirection;
    private Vector3 _wallRunDirection; // Направление вдоль стены
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _jumpsRemaining = maxJumps;
    }

    public void OnMove(InputValue val) => _move = val.Get<Vector2>();

    public void OnDash(InputValue val)
    {
        if (val.isPressed && _dashCooldown <= 0)
        {
            _dashVelocity = dashImpulse;
            _dashCooldown = 1f;
        }
    }

    public void OnJump(InputValue val)
    {
        if (val.isPressed)
        {
            _jumpPressedThisFrame = true; // Фиксируем нажатие для ApplyMovement

            if (_isWallRunning)
            {
                // WALL JUMP: Подбрасываем вверх
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                // Примечание: Импульс "от стены" мы обработаем в ApplyMovement через флаг
            }
            else if (_jumpsRemaining > 0)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _jumpsRemaining--;
            }
        }
    }


    void Update()
    {
        HandleWallRunDetection();
        HandleAcceleration();
        HandleGravityAndJump();
        ApplyMovement();
        if (_dashCooldown > 0) _dashCooldown -= Time.deltaTime; // Исправил опечатку из твоего кода
        // Сброс флага прыжка в конце кадра, если он не был использован
        // (Это гарантирует, что импульс сработает только один раз)
        // Но так как мы сбрасываем его внутри ApplyMovement после использования, здесь можно добавить очистку для безопасности:
        // _jumpPressedThisFrame = false; // Если хочешь, чтобы прыжок работал строго в момент нажатия
    }

    private void HandleWallRunDetection()
    {
        bool hitLeft = leftWallDetector.IsColliding;
        bool hitRight = rightWallDetector.IsColliding;
        // Условие Wall Run: Мы в воздухе + коснулись стены + движемся вперед
        if ((hitLeft || hitRight) && !_characterController.isGrounded)
        {
            _isWallRunning = true;

            // Берем нормаль той стены, которую нашли
            _wallNormal = hitLeft ? leftWallDetector.outHit.normal : rightWallDetector.outHit.normal;

            // Вычисляем направление ВДОЛЬ стены (перпендикулярно нормали и вектору Up)
            // Используем Cross product (векторное произведение)
            Vector3 wallTangent = Vector3.Cross(_wallNormal, Vector3.up);

            // Нам нужно выбрать правильное направление (вперед или назад вдоль стены), 
            // чтобы игрока не разворачивало назад при касании стены
            if (Vector3.Dot(wallTangent, _lastMoveDirection) < 0)
            {
                _wallRunDirection = -wallTangent;
            }
            else
            {
                _wallRunDirection = wallTangent;
            }
        }
        else
        {
            _isWallRunning = false;
            _wallRunDirection = Vector3.zero;
        }
    }

    private void HandleAcceleration()
    {
        float targetSpeed = _move.magnitude > 0 ? runSpeed : 0f;
        if (_move.magnitude > 0)
        {
            // Если мы бежим по стене, направление движения берется вдоль стены, а не от камеры
            Vector3 moveDir = _isWallRunning ? _wallRunDirection : (GetForward() * _move.y + GetRight() * _move.x).normalized;
            _lastMoveDirection = moveDir;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }
        _dashVelocity = Mathf.MoveTowards(_dashVelocity, 0f, 30f * Time.deltaTime);
    }

    private void HandleGravityAndJump()
    {
        if (_characterController.isGrounded)
        {
            if (_verticalVelocity < 0) _verticalVelocity = -2f;
            _jumpsRemaining = maxJumps;
        }
        // Если бежим по стене, уменьшаем силу гравитации (эффект левитации)
        float currentGravity = _isWallRunning ? gravity * wallRunGravityMultiplier : gravity;
        _verticalVelocity += currentGravity * Time.deltaTime;
    }

    private void ApplyMovement()
    {
        float totalSpeed = currentSpeed + _dashVelocity;
        // 1. Определяем горизонтальное движение (обычное или по стене)
        Vector3 horizontalMove = _isWallRunning ? _wallRunDirection * wallRunSpeed : _lastMoveDirection * totalSpeed;
        // 2. Вертикальное движение (гравитация/прыжок)
        Vector3 verticalMove = Vector3.up * _verticalVelocity; // Исправил опечатку
        // 3. Обработка импульса ОТ стены (Wall Jump Impulse)
        Vector3 wallJumpImpulse = Vector3.zero;
        if (_isWallRunning && _jumpPressedThisFrame)
        {
            // Толкаем игрока в сторону от нормали стены
            wallJumpImpulse = _wallNormal * wallJumpForce;
            // Сбрасываем флаг, чтобы импульс не применялся бесконечно
            _jumpPressedThisFrame = false;
        }
        // 4. Применяем итоговый вектор
        _characterController.Move((horizontalMove + verticalMove + wallJumpImpulse) * Time.deltaTime);
    }

    private Vector3 GetForward()
    {
        Vector3 forward = _cinemach.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }
    private Vector3 GetRight()
    {
        Vector3 right = _cinemach.transform.right;
        right.y = 0;
        return right.normalized;
    }
}
