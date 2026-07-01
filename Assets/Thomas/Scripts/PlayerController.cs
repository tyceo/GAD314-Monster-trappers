using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private Transform cameraHolder;

    private CharacterController characterController;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isGrounded;

    // Input System
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (cameraHolder != null)
                cameraHolder.GetComponentInChildren<Camera>().enabled = false;
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Create input actions at runtime
        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up",    "<Keyboard>/w")
            .With("Down",  "<Keyboard>/s")
            .With("Left",  "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        lookAction = new InputAction("Look", binding: "<Mouse>/delta");

        jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");

        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
    }

    public override void OnNetworkDespawn()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
    }

    private void HandleLook()
    {
        Vector2 lookDelta = lookAction.ReadValue<Vector2>();

        // Rotate player body horizontally
        transform.Rotate(Vector3.up * lookDelta.x * mouseSensitivity * Time.deltaTime * 100f);

        // Rotate camera vertically
        verticalRotation -= lookDelta.y * mouseSensitivity * Time.deltaTime * 100f;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    private void HandleMovement()
    {
        isGrounded = characterController.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Jump
        if (jumpAction.WasPressedThisFrame() && isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
}