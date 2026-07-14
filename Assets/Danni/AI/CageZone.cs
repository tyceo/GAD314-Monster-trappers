using Unity.Netcode;
using UnityEngine;

public class CageZone : NetworkBehaviour
{
    private bool isMonsterInZone = false;
    private bool isPlayerInZone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        
        if (other.GetComponent<MonsterController>() != null)
        {
            isMonsterInZone = true;
            UpdateCageStatus();
            Debug.Log("Monster entered cage");
        }
        else if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            UpdateCageStatus();
            Debug.Log("Player entered cage");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsServer) return;
        
        if (other.GetComponent<MonsterController>() != null)
        {
            isMonsterInZone = false;
            UpdateCageStatus();
            Debug.Log("Monster exited cage");
        }
        else if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            UpdateCageStatus();
            Debug.Log("Player exited cage");
        }
    }

    private void UpdateCageStatus()
    {
        // Monster can only be considered "in cage" if monster is in zone AND player is NOT in zone
        bool monsterInCage = isMonsterInZone && !isPlayerInZone;
        GameSessionManager.Instance.SetMonsterInCage(monsterInCage);
        Debug.Log($"Cage status updated: Monster={isMonsterInZone}, Player={isPlayerInZone}, MonsterInCage={monsterInCage}");
    }
}