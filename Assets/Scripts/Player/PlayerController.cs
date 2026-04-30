using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float currentSpeed;
    public float runSpeed;

    [Header("Jump Settings")]
    public float jumpHeight;
    public int maxJumps;
    public float gravity;     
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cinemach;

    private Vector2 _move;
    private float _verticalVelocity;    // Текущая вертикальная скорость (падение/прыжок)
    private int _jumpsRemaining;       // Сколько прыжков осталось в воздухе

    void Start()
    {
        currentSpeed = runSpeed;
        _jumpsRemaining = maxJumps; // Инициализируем количество прыжков
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputValue val)
    {
        _move = val.Get<Vector2>();
    }

    // Метод для обработки нажатия кнопки прыжка (нужно добавить Action "Jump" в Input Actions)
    public void OnJump(InputValue val)
    {
        if (val.isPressed && _jumpsRemaining > 0)
        {
            // Формула физики: v = sqrt(h * -2 * g)
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _jumpsRemaining--;
        }
    }

    void Update()
    {
        // 1. ЛОГИКА ПЕРЕМЕЩЕНИЯ (Твоя оригинальная логика)
        Vector3 direction = (GetForward() * _move.y) + (GetRight() * _move.x);
        _characterController.Move(direction * Time.deltaTime * currentSpeed);

        // 2. ЛОГИКА ГРАВИТАЦИИ И ПРЫЖКА
        // Проверка: если приземлились, сбрасываем прыжки и скорость падения
        if (_characterController.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f; // Небольшое давление вниз, чтобы персонаж "прилипал" к земле
            _jumpsRemaining = maxJumps;
        }

        // Применяем гравитацию к вертикальной скорости
        _verticalVelocity += gravity * Time.deltaTime;

        // Двигаем персонажа по вертикали (отдельно от горизонтального движения)
        _characterController.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
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
