using UnityEngine;

/// Minimal first-person controller for demo testing.
/// Plays the role of PlayerMotor for WeaponManager and HUDController:
/// exposes activeWeapon, currentSpeed, AddRecoil, ApplyExternalImpulse.
/// Use this when the full PlayerMotor rig is not yet wired up.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerHealth))]
public class TestPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float lookSensitivity = 2.5f;
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
        float mx = Input.GetAxis("Mouse X") * lookSensitivity;
        float my = Input.GetAxis("Mouse Y") * lookSensitivity;

        transform.Rotate(0f, mx, 0f);

        pitch = Mathf.Clamp(pitch - my, -85f, 85f);
        if (cameraHolder != null)
            cameraHolder.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleMove()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 wish = (transform.right * h + transform.forward * v).normalized;

        if (cc.isGrounded)
        {
            if (verticalVelocity.y < 0f) verticalVelocity.y = -2f;
            if (Input.GetKeyDown(KeyCode.Space))
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

        if (Input.GetKeyDown(KeyCode.Alpha1)) weaponManager.SwitchWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) weaponManager.SwitchWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) weaponManager.SwitchWeapon(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) weaponManager.SwitchWeapon(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) weaponManager.SwitchWeapon(4);
        if (Input.GetKeyDown(KeyCode.Alpha6)) weaponManager.SwitchWeapon(5);
        if (Input.GetKeyDown(KeyCode.Q)) weaponManager.QuickSwap();

        activeWeapon = weaponManager.ActiveWeapon;
    }

    private void HandleWeaponInput()
    {
        if (activeWeapon == null) return;

        bool attack = Input.GetMouseButton(0);
        bool skill = Input.GetMouseButton(1);
        activeWeapon.HandleContinuousInput(attack, skill);
    }
}
