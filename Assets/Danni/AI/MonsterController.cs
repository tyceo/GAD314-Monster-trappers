using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MonsterController : NetworkBehaviour
{
    public AStar3D astar;
    public Transform player;

    public float detectionRange = 15f;
    public float playerViewAngle = 60f;
    public float repathInterval = 0.5f;
    public float moveSpeed = 4f;
    public float rotateSpeed = 6f;
    public float reachDistance = 0.5f;
    public float catchDistance = 1f;

    private List<Vector3> currentPath;
    private int waypointIndex;
    private float repathTimer;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"[MonsterController] OnNetworkSpawn called. IsServer: {IsServer}");
        
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;

        if (!IsServer) 
        { 
            Debug.Log("[MonsterController] Not server, disabling component");
            enabled = false; 
            return; 
        }
        
        Debug.Log("[MonsterController] Running on server, initializing");
        
        if (!astar) astar = FindFirstObjectByType<AStar3D>();
        
        Debug.Log($"[MonsterController] AStar found: {astar != null}");
    }

    private void Update()
    {
        if (!IsServer)
        {
            Debug.LogWarning("[MonsterController] Update called but not server!");
            return;
        }
        
        if (GameSessionManager.Instance != null && !GameSessionManager.Instance.IsPlaying)
        {
            Debug.LogWarning("[MonsterController] Game session not playing yet");
            return;
        }
        
        if (GameSessionManager.Instance == null)
        {
            Debug.LogWarning("[MonsterController] GameSessionManager.Instance is null!");
            return;
        }

        Debug.Log("[MonsterController] Update running - looking for player");

        if (!player)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found)
            {
                player = found.transform;
                Debug.Log($"[MonsterController] Found player: {player.name}");
            }
            else
            {
                Debug.LogWarning("[MonsterController] No GameObject with 'Player' tag found!");
            }
        }
        if (player == null || astar == null)
        {
            if (player == null) Debug.LogWarning("[MonsterController] Player is null");
            if (astar == null) Debug.LogWarning("[MonsterController] AStar is null");
            return;
        }

        float sqrDist = (player.position - transform.position).sqrMagnitude;

        if (sqrDist <= catchDistance * catchDistance)
        {
            GameSessionManager.Instance.NotifyPlayerCaught();
            return;
        }

        bool inRange = sqrDist <= detectionRange * detectionRange;
        bool playerLooking = inRange && IsPlayerLookingAtMonster();
        bool isChasing = inRange && !playerLooking;

        if (isChasing)
        {
            repathTimer -= Time.deltaTime;
            if (repathTimer <= 0f)
            {
                repathTimer = repathInterval;
                Repath();
            }
            FollowPath();
            FacePlayer();
        }
    }

    private bool IsPlayerLookingAtMonster()
    {
        Vector3 toMonster = transform.position - player.position;
        toMonster.y = 0f;
        if (toMonster.sqrMagnitude < 0.0001f) return true;
        toMonster.Normalize();

        Vector3 playerForward = player.forward;
        playerForward.y = 0f;
        playerForward.Normalize();

        float dot = Vector3.Dot(playerForward, toMonster);
        float cosHalfAngle = Mathf.Cos(playerViewAngle * 0.5f * Mathf.Deg2Rad);
        bool withinCone = dot >= cosHalfAngle;

        return withinCone && HasLineOfSight();
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = player.position + Vector3.up * 1.6f;
        Vector3 targetPoint = transform.position + Vector3.up * 1f;
        Vector3 dir = targetPoint - origin;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude))
            return hit.transform == transform || hit.transform.IsChildOf(transform);

        return true;
    }

    private void Repath()
    {
        if (!astar.grid) astar.grid = FindFirstObjectByType<Grid3D>();
        astar.start = transform;
        astar.target = player;
        astar.RunAStarImmediately();
        currentPath = astar.GetWorldPath();
        waypointIndex = 0;
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning("[MonsterController] No path to follow!");
            return;
        }
        
        if (waypointIndex >= currentPath.Count)
        {
            Debug.Log("[MonsterController] Reached end of path");
            return;
        }

        Vector3 targetPos = currentPath[waypointIndex];
        targetPos.y = transform.position.y;
        Vector3 toTarget = targetPos - transform.position;

        Debug.Log($"[MonsterController] Moving to waypoint {waypointIndex}/{currentPath.Count - 1}, distance: {toTarget.magnitude:F2}");

        if (toTarget.magnitude < reachDistance)
        {
            waypointIndex++;
            Debug.Log($"[MonsterController] Reached waypoint, moving to next: {waypointIndex}");
            return;
        }

        transform.position += toTarget.normalized * moveSpeed * Time.deltaTime;
    }

    private void FacePlayer()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotateSpeed);
    }

    public void ResetToSpawn()
    {
        if (!IsServer) return;
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        currentPath = null;
        waypointIndex = 0;
    }
}