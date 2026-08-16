using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public enum MonsterFollowMode { LookAwayFollow, LookAtFollow }
public class MonsterController : NetworkBehaviour
{
  public AStar3D astar;
    public Transform player;
 
    [Header("Follow Behavior")]
    public MonsterFollowMode followMode = MonsterFollowMode.LookAwayFollow;
 
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
 
    private MonsterAnimator monsterAnimator;
    private bool isAttacking;
    private bool isTrapped;
 
    public override void OnNetworkSpawn()
    {
        Debug.Log($"[MonsterController] OnNetworkSpawn called. IsServer: {IsServer}");
 
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
 
        if (!IsServer)
        {
            enabled = false;
            return;
        }
        if (!astar) astar = FindFirstObjectByType<AStar3D>();
        if (!monsterAnimator) monsterAnimator = GetComponentInChildren<MonsterAnimator>(); 
 
        monsterAnimator?.PlayIdle(); 
    }
 
    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
 
        if (GameSessionManager.Instance != null && !GameSessionManager.Instance.IsPlaying)
        {
            return;
        }
 
        if (GameSessionManager.Instance == null)
        {
            return;
        }
 
        if (isTrapped) return; 
 
        if (!player)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found)
            {
                player = found.transform;
            }
        }
        if (player == null || astar == null)
            return;
 
        if (isAttacking) return; 
 
        float sqrDist = (player.position - transform.position).sqrMagnitude;
 
        if (sqrDist <= catchDistance * catchDistance)
        {
            BeginAttack(); 
            return;
        }
 
        bool inRange = sqrDist <= detectionRange * detectionRange;
        bool playerLooking = inRange && IsPlayerLookingAtMonster();
 
        // chase condition now depends on followMode
        bool isChasing = followMode == MonsterFollowMode.LookAwayFollow
            ? inRange && !playerLooking
            : inRange && playerLooking;
 
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
            monsterAnimator?.PlayRun(); 
        }
        else
        {
            monsterAnimator?.PlayIdle(); 
        }
    }
 
    private void BeginAttack()
    {
        isAttacking = true;
        FacePlayer();
        monsterAnimator?.PlayAttack(OnAttackAnimationFinished);
    }
    
    private void OnAttackAnimationFinished()
    {
        isAttacking = false;
        GameSessionManager.Instance?.NotifyPlayerCaught();
    }
 
    //called by GameSessionManager when the monster is caged
    public void PlayTrappedAnimation(Action onFinished)
    {
        if (!IsServer) return;
        isTrapped = true;
        currentPath = null;
        monsterAnimator?.PlayDeath(onFinished);
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
        if (!astar.grid)
        {
            Debug.LogWarning("[MonsterController] No Grid3D found in scene!");
            return;
        }

        astar.start = transform;
        astar.target = player;
        astar.RunAStarImmediately();
        currentPath = astar.GetWorldPath();
        waypointIndex = 0;

        Debug.Log($"[MonsterController] Repath: grid={astar.grid.name}, pathPoints={(currentPath != null ? currentPath.Count : 0)}");
    }
 
    private void FollowPath()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            return;
        }
 
        if (waypointIndex >= currentPath.Count)
        {
            return;
        }
 
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
 
    public void ResetToSpawn()
    {
        if (!IsServer) return;
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;
        currentPath = null;
        waypointIndex = 0;
        isAttacking = false; 
        isTrapped = false;
        monsterAnimator?.ResetAnimator();
    }
}