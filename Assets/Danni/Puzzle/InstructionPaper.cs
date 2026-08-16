using Unity.Netcode;
using UnityEngine;

public class InstructionPaper : MonoBehaviour
{
    public string title;
    [TextArea(3, 8)]
    public string instructionText;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.IsOwner) return;
        if (NetworkManager.Singleton.LocalClient == null ||
            NetworkManager.Singleton.LocalClient.PlayerObject != netObj) return;

        InstructionPopup.Instance.Show(title, instructionText);
    }
}
