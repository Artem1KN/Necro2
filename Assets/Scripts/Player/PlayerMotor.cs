using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerMotor : MonoBehaviour
{
    [Header("Movement Settings")]
    public float runSpeed = 10f;
    public float acceleration = 10f;
    public float deceleration = 8f;
    public float currentSpeed;

    [Header("Dash Settings")]
    public float dashImpulse = 30f;
    private float _dashCooldown = 0f;
    private float _dashVelocity;

    [Header("Jump & Gravity")]
    public float jumpHeight = 4f;
    public float gravity = -19.62f;
    private float _verticalVelocity;
    private int _jumpsRemaining;
    public int maxJumps = 2;
    private bool _jumpPressedThisFrame = false;

    [Header("Wall Checkers")]
    public CollisionDetectorRaycast leftWallDetector;
    public CollisionDetectorRaycast rightWallDetector;

    [Header("Wall Run Settings")]
    public float wallRunSpeed = 7f;
    public float wallJumpForce = 10f;
    public float wallRunGravityMultiplier = 0.2f;
    private bool _isWallRunning = false;
    private Vector3 _wallNormal;
    private Vector3 _wallRunDirection;

    [Header("FOV Settings")] // --- НОВОЕ: Настройки FOV ---
    [SerializeField] private float baseFOV = 60f;      // Обычный угол обзора
    [SerializeField] private float maxFOV = 80f;       // Угол при рывке/стене
    [SerializeField] private float fovTransitionSpeed = 10f; // Скорость изменения FOV

    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cinemach;

    private Vector2 _move;
    private Vector3 _lastMoveDirection;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _jumpsRemaining = maxJumps;

        // Устанавливаем начальный FOV при старте
        if (_cinemach != null) _cinemach.Lens.FieldOfView = baseFOV;
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
            _jumpPressedThisFrame = true;
            if (_isWallRunning)
            {
                _isWallRunning = false;
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
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
        SyncDetectorsToCamera();
        HandleWallRunDetection();
        HandleAcceleration();
        HandleGravityAndJump();
        ApplyMovement();
        HandleFOV(); // --- НОВОЕ: Вызов управления FOV ---
        if (_dashCooldown > 0) _dashCooldown -= Time.deltaTime;

    }

    // --- НОВОЕ: Логика изменения FOV ---
    private void HandleFOV()
    {
        if (_cinemach == null) return;

        // Определяем целевой FOV в зависимости от состояния
        float targetFOV = baseFOV;

        // Если мы в рывке (dashVelocity еще высокий) или бежим по стене
        if (_dashVelocity > 1f || _isWallRunning)
        {
            targetFOV = maxFOV;
        }

        // Плавно переходим от текущего FOV к целевому
        _cinemach.Lens.FieldOfView = Mathf.Lerp(_cinemach.Lens.FieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
    }

    private void HandleWallRunDetection()
    {
        bool hitLeft = leftWallDetector.IsColliding;
        bool hitRight = rightWallDetector.IsColliding;

        if ((hitLeft || hitRight) && !_characterController.isGrounded)
        {
            bool wasWallRunning = _isWallRunning;
            _isWallRunning = true;
            _wallNormal = hitLeft ? leftWallDetector.outHit.normal : rightWallDetector.outHit.normal;

            if (!wasWallRunning)
            {
                _verticalVelocity = 0;
            }

            Vector3 wallTangent = Vector3.Cross(_wallNormal, Vector3.up);
            if (Vector3.Dot(wallTangent, _lastMoveDirection) < 0)
                _wallRunDirection = -wallTangent;
            else
                _wallRunDirection = wallTangent;
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
            Vector3 moveDir = _isWallRunning ? _wallRunDirection * wallRunSpeed : (GetForward() * _move.y + GetRight() * _move.x).normalized;
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

        if (_isWallRunning)
        {
            _verticalVelocity = 0;
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        float totalSpeed = currentSpeed + _dashVelocity;
        Vector3 horizontalMove = _isWallRunning ? _wallRunDirection * wallRunSpeed : _lastMoveDirection * totalSpeed;
        Vector3 verticalMove = Vector3.up * _verticalVelocity;
        Vector3 wallJumpImpulse = Vector3.zero;

        if (_jumpPressedThisFrame && _isWallRunning)
        {
            wallJumpImpulse = _wallNormal * wallJumpForce;
            _jumpPressedThisFrame = false;
        }
        else if (_jumpPressedThisFrame)
        {
            _jumpPressedThisFrame = false;
        }

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

    private void SyncDetectorsToCamera()
    {
        if (_cinemach == null) return;
        // Получаем текущий поворот камеры по оси Y
        float targetYRotation = _cinemach.transform.eulerAngles.y;
        // Создаем новый кватернион: только Y, X и Z равны 0
        Quaternion targetRotation = Quaternion.Euler(0, targetYRotation, 0);
        // Применяем вращение к каждому детектору напрямую
        if (leftWallDetector != null) leftWallDetector.transform.rotation = targetRotation;
        if (rightWallDetector != null) rightWallDetector.transform.rotation = targetRotation;
    }

}
