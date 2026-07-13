using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

/*public class PlayerController : NetworkBehaviour
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
    
    
    [Header("Keys and Doors")]
    private GameObject greenDoor;
    private GameObject blueDoor;
    private GameObject purpleDoor;
    private GameObject orangeDoor;
    private GameObject greenKey;
    private GameObject blueKey;
    private GameObject purpleKey;
    private GameObject orangeKey;
    
    [Header("Respawn")]
    [SerializeField] private Vector3 respawnPosition = new Vector3(3.35f, 1.83f, -4.38f);

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            if (cameraHolder != null)
                cameraHolder.GetComponentInChildren<Camera>().enabled = false;
            enabled = false;
            return;
        }
        
        // Enable fog
        RenderSettings.fog = true;

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
    private void Start()
    {
        // Find all keys and doors by name
        greenDoor = GameObject.Find("GreenDoor");
        blueDoor = GameObject.Find("BlueDoor");
        purpleDoor = GameObject.Find("PurpleDoor");
        orangeDoor = GameObject.Find("OrangeDoor");
        
        greenKey = GameObject.Find("GreenKey");
        blueKey = GameObject.Find("BlueKey");
        purpleKey = GameObject.Find("PurpleKey");
        orangeKey = GameObject.Find("OrangeKey");
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
    
    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;
        
        // Check if player touched a death trigger
        if (other.CompareTag("PlayerDie"))
        {
            RespawnPlayer();
            return;
        }

        // Check if player touched a key
        if (other.gameObject == greenKey)
        {
            CollectKeyServerRpc("green");
        }
        else if (other.gameObject == blueKey)
        {
            CollectKeyServerRpc("blue");
        }
        else if (other.gameObject == purpleKey)
        {
            CollectKeyServerRpc("purple");
        }
        else if (other.gameObject == orangeKey)
        {
            CollectKeyServerRpc("orange");
        }
    }
    private void RespawnPlayer()
    {
        // Disable CharacterController temporarily to allow position change
        characterController.enabled = false;
        transform.position = respawnPosition;
        velocity = Vector3.zero; // Reset velocity
        characterController.enabled = true;
    }

    [ServerRpc]
    private void CollectKeyServerRpc(string color)
    {
        // This runs on the server and will be synced to all clients
        GameObject key = null;
        GameObject door = null;

        switch (color)
        {
            case "green":
                key = GameObject.Find("GreenKey");
                door = GameObject.Find("GreenDoor");
                break;
            case "blue":
                key = GameObject.Find("BlueKey");
                door = GameObject.Find("BlueDoor");
                break;
            case "purple":
                key = GameObject.Find("PurpleKey");
                door = GameObject.Find("PurpleDoor");
                break;
            case "orange":
                key = GameObject.Find("OrangeKey");
                door = GameObject.Find("OrangeDoor");
                break;
        }

        if (key != null)
        {
            NetworkObject keyNetObj = key.GetComponent<NetworkObject>();
            if (keyNetObj != null)
            {
                keyNetObj.Despawn();
            }
            else
            {
                Destroy(key);
            }
        }

        if (door != null)
        {
            NetworkObject doorNetObj = door.GetComponent<NetworkObject>();
            if (doorNetObj != null)
            {
                doorNetObj.Despawn();
            }
            else
            {
                Destroy(door);
            }
        }
    }
}*/

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

    [Header("Keys")]
    private GameObject greenKey;
    private GameObject blueKey;
    private GameObject purpleKey;
    private GameObject orangeKey;

    [Header("Respawn")]
    [SerializeField] private Vector3 respawnPosition = new Vector3(3.35f, 1.83f, -4.38f);

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

    private void Start()
    {
        greenKey = GameObject.Find("GreenKey");
        blueKey = GameObject.Find("BlueKey");
        purpleKey = GameObject.Find("PurpleKey");
        orangeKey = GameObject.Find("OrangeKey");
    }

    private void Update()
    {
        HandleLook();
        HandleMovement();
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
            return;
        }

        if (other.gameObject == greenKey) CollectKeyServerRpc(DoorColor.Green);
        else if (other.gameObject == blueKey) CollectKeyServerRpc(DoorColor.Blue);
        else if (other.gameObject == purpleKey) CollectKeyServerRpc(DoorColor.Purple);
        else if (other.gameObject == orangeKey) CollectKeyServerRpc(DoorColor.Orange);
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

    [ServerRpc]
    private void CollectKeyServerRpc(DoorColor color)
    {
        GameObject key = null;
        switch (color)
        {
            case DoorColor.Green: key = GameObject.Find("GreenKey"); break;
            case DoorColor.Blue: key = GameObject.Find("BlueKey"); break;
            case DoorColor.Purple: key = GameObject.Find("PurpleKey"); break;
            case DoorColor.Orange: key = GameObject.Find("OrangeKey"); break;
        }

        if (key != null)
        {
            NetworkObject keyNetObj = key.GetComponent<NetworkObject>();
            if (keyNetObj != null) keyNetObj.Despawn();
            else Destroy(key);
        }

        GameSessionManager.Instance.ActivateColorServerRpc(color);
    }
}