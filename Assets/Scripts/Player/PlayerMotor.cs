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
    public int maxJumps = 2;
    
    [Header("Handlers")]
    [SerializeField] private WallRunHandler wallRunHandler;
    [SerializeField] private CameraEffectsHandler cameraEffectsHandler;

    [Header("References")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cinemach;

    // Private State
    private Vector2 _move;
    private Vector3 _lastMoveDirection;
    private float _verticalVelocity;
    private int _jumpsRemaining;
    private bool _jumpRequestedThisFrame;
    private bool _isWallRunning; // Вынесено из методов в поле класса

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _jumpsRemaining = maxJumps;
    }

    // --- Input Handlers ---

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
        Debug.Log("Test - jump");
        if (val.isPressed)
        {
            _jumpRequestedThisFrame = true;
        }
    }

    // --- Core Logic ---

    void Update()
    {
        UpdateWallRunState();
        HandleAcceleration();
        HandleGravityAndJump();
        ApplyMovement();
        HandlePostMovementEffects();

        // Cooldown management
        if (_dashCooldown > 0) _dashCooldown -= Time.deltaTime;
    }

    private void UpdateWallRunState()
    {
        if (wallRunHandler != null)
        {
            wallRunHandler.SyncDetectorsToCamera(_cinemach != null ? _cinemach.transform : null);
            wallRunHandler.UpdateWallRunLogic(_lastMoveDirection, ref _verticalVelocity);
            _isWallRunning = wallRunHandler.IsWallRunningState;
        }
        else
        {
            _isWallRunning = false;
        }
    }

    private void HandleAcceleration()
    {
        float targetSpeed = _move.magnitude > 0 ? runSpeed : 0f;

        if (_move.magnitude > 0)
        {
            // Если бежим по стене, используем направление стены, иначе обычный ввод
            Vector3 moveDir = _isWallRunning 
                ? wallRunHandler.WallRunDirection * wallRunHandler.WallRunSpeed 
                : (GetForward() * _move.y + GetRight() * _move.x).normalized;

            _lastMoveDirection = moveDir;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }

        // Decay dash velocity
        _dashVelocity = Mathf.MoveTowards(_dashVelocity, 0f, 30f * Time.deltaTime);
    }

    private void HandleGravityAndJump()
    {
        // Grounded logic
        if (_characterController.isGrounded)
        {
            if (_verticalVelocity < 0) _verticalVelocity = -2f;
            _jumpsRemaining = maxJumps;
        }

        // Jump Logic
        if (_jumpRequestedThisFrame)
        {
            if (_isWallRunning)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else if (_jumpsRemaining > 0)
            {
                _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                _jumpsRemaining--;
            }
            _jumpRequestedThisFrame = false; // Reset request after processing
        }

        // Gravity Logic
        if (_isWallRunning)
        {
            _verticalVelocity = 0; // Neutralize gravity during wall run
        }
        else
        {
            _verticalVelocity += gravity * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        float totalHorizontalSpeed = currentSpeed + _dashVelocity;
        
        // Determine horizontal direction
        Vector3 horizontalMove = _isWallRunning 
            ? wallRunHandler.WallRunDirection * wallRunHandler.WallRunSpeed 
            : _lastMoveDirection * totalHorizontalSpeed;

        Vector3 verticalMove = Vector3.up * _verticalVelocity;
        
        // Wall Jump Impulse
        Vector3 wallJumpImpulse = Vector3.zero;
        if (_isWallRunning && _jumpRequestedThisFrame) // Note: jump logic handled in HandleGravity, but we use impulse here
        {
            // This part is tricky because OnJump sets the flag. 
            // If you want a "kick off" effect, it's best kept as a separate physics force.
            // For now, keeping your original logic structure:
            wallJumpImpulse = wallRunHandler.WallNormal * wallRunHandler.WallJumpForce;
        }

        _characterController.Move((horizontalMove + verticalMove + wallJumpImpulse) * Time.deltaTime);
    }

    private void HandlePostMovementEffects()
    {
        if (cameraEffectsHandler != null)
        {
            cameraEffectsHandler.HandleFOV(_isWallRunning, _dashVelocity);
        }
    }

    // --- Helpers ---

    private Vector3 GetForward()
    {
        if (_cinemach == null) return Vector3.forward;
        Vector3 forward = _cinemach.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetRight()
    {
        if (_cinemach == null) return Vector3.right;
        Vector3 right = _cinemach.transform.right;
        right.y = 0;
        return right.normalized;
    }
}