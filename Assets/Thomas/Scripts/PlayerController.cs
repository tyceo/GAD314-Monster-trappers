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

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;

    [Header("Respawn")]
    [SerializeField] private Vector3 respawnPosition = new Vector3(3.35f, 1.83f, -4.38f);

    [Header("Interaction")]
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private LayerMask interactableLayer;
    private InputAction interactAction;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (cameraHolder != null)
                cameraHolder.GetComponentInChildren<Camera>().enabled = false;
            enabled = false;
            return;
        }

        RenderSettings.fog = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        moveAction = new InputAction("Move", binding: "<Gamepad>/leftStick");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        lookAction = new InputAction("Look", binding: "<Mouse>/delta");
        jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");

        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonSouth");

        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        interactAction.Enable();
    }

    public void GameEnded()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public override void OnNetworkDespawn()
    {
        moveAction?.Disable();
        lookAction?.Disable();
        jumpAction?.Disable();
        interactAction?.Disable();
    }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
        HandleInteraction();
    }

    private void HandleLook()
    {
        Vector2 lookDelta = lookAction.ReadValue<Vector2>();

        transform.Rotate(Vector3.up * lookDelta.x * mouseSensitivity * Time.deltaTime * 100f);

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

        if (jumpAction.WasPressedThisFrame() && isGrounded)
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        if (other.CompareTag("PlayerDie"))
        {
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        characterController.enabled = false;
        transform.position = respawnPosition;
        velocity = Vector3.zero;
        characterController.enabled = true;
    }

    public void ServerRequestedRespawn()
    {
        if (!IsOwner) return;
        RespawnPlayer();
    }

    private void HandleInteraction()
    {
        if (interactAction.WasPressedThisFrame())
        {
            Ray ray = new Ray(cameraHolder.position, cameraHolder.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableLayer))
            {
                if (hit.collider.CompareTag("Switch"))
                {
                    Switchboard switchboard = hit.collider.GetComponentInParent<Switchboard>();

                    if (switchboard != null)
                    {
                        string switchName = hit.collider.gameObject.name;
                        int switchNumber = 0;

                        if (switchName.Contains("1")) switchNumber = 1;
                        else if (switchName.Contains("2")) switchNumber = 2;
                        else if (switchName.Contains("3")) switchNumber = 3;
                        else if (switchName.Contains("4")) switchNumber = 4;

                        if (switchNumber > 0)
                        {
                            switchboard.ToggleSwitchServerRpc(switchNumber);
                            Debug.Log($"[PlayerController] Toggled switch {switchNumber} on {switchboard.GetBoardColor()} switchboard");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("[PlayerController] Switch has no Switchboard parent!");
                    }
                }
            }
        }
    }
}