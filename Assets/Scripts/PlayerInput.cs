using UnityEngine;

public class PlayerInput : MonoBehaviour
    {
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    [HideInInspector] public Vector2 moveInput;
    [HideInInspector] public Vector2 lookInput;
    
    private float xRotation = 0f;

    void Start()
    {
        // Скрываем курсор
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Получаем ввод движения
        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        // Получаем ввод мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // ИСПРАВЛЕНО: Убрана ошибка "import/x"
        lookInput.x = mouseX;
        lookInput.y = mouseY;

        // Логика вращения головы (вверх-вниз)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // ДОБАВЛЕНО: Логика поворота тела (влево-вправо)
        // Это позволит персонажу вращаться вокруг своей оси
        transform.Rotate(Vector3.up * mouseX);
    }
}
