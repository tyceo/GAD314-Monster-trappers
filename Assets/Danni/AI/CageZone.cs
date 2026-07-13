using Unity.Netcode;
using UnityEngine;

public class CageZone : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.GetComponent<MonsterController>() != null)
            GameSessionManager.Instance.SetMonsterInCage(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        if (other.GetComponent<MonsterController>() != null)
            GameSessionManager.Instance.SetMonsterInCage(false);
    }
}