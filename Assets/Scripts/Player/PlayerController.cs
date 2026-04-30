using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float currentSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    [SerializeField] private CharacterController _characterController;
    [SerializeField] private CinemachineCamera _cinemach;

    private Vector2 _move;

    void Start()
    {
        currentSpeed = walkSpeed;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnMove(InputValue val)
    {
        _move = val.Get<Vector2>();
    }

    public void OnSprint(InputValue val)
    {
        if (val.Get<float>() > 0.5f)
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
    }

    void Update()
    {
        // Мы создаем вектор направления, складывая forward и right, 
        // и умножаем его на ввод (x и y)
        Vector3 direction = (GetForward() * _move.y) + (GetRight() * _move.x);

        _characterController.Move(direction * Time.deltaTime * currentSpeed);
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
