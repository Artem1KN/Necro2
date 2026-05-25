using UnityEngine;
using UnityEngine.InputSystem;

/// Minimal first-person controller for demo testing.
/// Plays the role of PlayerMotor for WeaponManager and HUDController:
/// exposes activeWeapon, currentSpeed, AddRecoil, ApplyExternalImpulse.
/// Uses the new Input System (Keyboard.current, Mouse.current).
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerHealth))]
public class TestPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float lookSensitivity = 0.15f;
    public float jumpHeight = 1.8f;
    public float gravity = -19.62f;

    [Header("Camera")]
    public Transform cameraHolder;

    [Header("Weapons")]
    public WeaponManager weaponManager;
    [HideInInspector] public WeaponBase activeWeapon;

    [Header("Recoil")]
    public RecoilController recoilController;

    public float currentSpeed { get; private set; }

    private CharacterController cc;
    private Vector3 verticalVelocity;
    private Vector3 pendingImpulse;
    private float pitch;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleLook();
        HandleMove();
        HandleWeaponSwitch();
        HandleWeaponInput();
    }

    public void AddRecoil(float strength, float duration)
    {
        if (recoilController != null) recoilController.AddKick(strength, duration);
    }

    public void ApplyExternalImpulse(Vector3 impulse)
    {
        pendingImpulse += impulse;
    }

    private void HandleLook()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 delta = mouse.delta.ReadValue() * lookSensitivity;

        transform.Rotate(0f, delta.x, 0f);

        pitch = Mathf.Clamp(pitch - delta.y, -85f, 85f);
        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        var kb = Keyboard.current;
        float h = 0f, v = 0f;
        if (kb != null)
        {
            if (kb.aKey.isPressed) h -= 1f;
            if (kb.dKey.isPressed) h += 1f;
            if (kb.sKey.isPressed) v -= 1f;
            if (kb.wKey.isPressed) v += 1f;
        }

        Vector3 wish = (transform.right * h + transform.forward * v).normalized;

        if (cc.isGrounded)
        {
            if (verticalVelocity.y < 0f) verticalVelocity.y = -2f;
            if (kb != null && kb.spaceKey.wasPressedThisFrame)
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity.y += gravity * Time.deltaTime;

        Vector3 horizontal = wish * moveSpeed + pendingImpulse;
        Vector3 motion = horizontal + Vector3.up * verticalVelocity.y;

        cc.Move(motion * Time.deltaTime);

        currentSpeed = new Vector3(horizontal.x, 0f, horizontal.z).magnitude;

        pendingImpulse = Vector3.Lerp(pendingImpulse, Vector3.zero, 4f * Time.deltaTime);
    }

    private void HandleWeaponSwitch()
    {
        if (weaponManager == null) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.digit1Key.wasPressedThisFrame) weaponManager.SwitchWeapon(0);
        if (kb.digit2Key.wasPressedThisFrame) weaponManager.SwitchWeapon(1);
        if (kb.digit3Key.wasPressedThisFrame) weaponManager.SwitchWeapon(2);
        if (kb.digit4Key.wasPressedThisFrame) weaponManager.SwitchWeapon(3);
        if (kb.digit5Key.wasPressedThisFrame) weaponManager.SwitchWeapon(4);
        if (kb.digit6Key.wasPressedThisFrame) weaponManager.SwitchWeapon(5);
        if (kb.qKey.wasPressedThisFrame) weaponManager.QuickSwap();

        activeWeapon = weaponManager.ActiveWeapon;
    }

    private void HandleWeaponInput()
    {
        if (activeWeapon == null) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        bool attack = mouse.leftButton.isPressed;
        bool skill = mouse.rightButton.isPressed;
        activeWeapon.HandleContinuousInput(attack, skill);
    }
}
