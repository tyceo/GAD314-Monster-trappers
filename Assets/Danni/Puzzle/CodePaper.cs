using Unity.Netcode;
using UnityEngine;

public class CodePaper : MonoBehaviour
{
    public DoorColor color;
    public string code;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkObject netObj = other.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.IsOwner) return;
        if (NetworkManager.Singleton.LocalClient == null ||
            NetworkManager.Singleton.LocalClient.PlayerObject != netObj) return;

        CodePopup.Instance.Show(color, code);
    }
}