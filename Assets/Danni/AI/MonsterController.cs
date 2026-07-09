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

    private List<Vector3> currentPath;
    private int waypointIndex;
    private float repathTimer;

    private void Start()
    {
        if (!IsServer) { enabled = false; return; }
        if (!astar) astar = FindFirstObjectByType<AStar3D>();
        if (!player)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found) player = found.transform;
        }
    }

    private void Update()
    {
        if (!IsServer || player == null || astar == null) return;

        bool inRange = IsPlayerInRange();
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

    private bool IsPlayerInRange()
    {
        float sqrDist = (player.position - transform.position).sqrMagnitude;
        return sqrDist <= detectionRange * detectionRange;
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
        astar.start = transform;
        astar.target = player;
        astar.RunAStarImmediately();
        currentPath = astar.GetWorldPath();
        waypointIndex = 0;
    }

    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0 || waypointIndex >= currentPath.Count) return;

        Vector3 targetPos = currentPath[waypointIndex];
        targetPos.y = transform.position.y;
        Vector3 toTarget = targetPos - transform.position;

        if (toTarget.magnitude < reachDistance)
        {
            waypointIndex++;
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
}