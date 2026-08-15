using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : NetworkBehaviour
{
    public Camera interactCamera;
    public float interactRange = 3f;
    public LayerMask interactMask = ~0;

    private InputAction interactAction;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        interactAction = new InputAction("Interact", binding: "<Keyboard>/e");
        interactAction.performed += _ => TryInteract();
        interactAction.Enable();
    }

    public override void OnNetworkDespawn()
    {
        interactAction?.Disable();
    }

    private void TryInteract()
    {
        if (!interactCamera) return;

        Ray ray = interactCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null) interactable.Interact();
        }
    }
}
