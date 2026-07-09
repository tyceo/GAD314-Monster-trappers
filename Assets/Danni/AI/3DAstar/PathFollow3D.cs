using System.Collections.Generic;
using UnityEngine;

public class PathFollow3D : MonoBehaviour
{
      [Header("Refs")]
    public AStar3D astar;
    public Transform goal;

    [Header("Movement")]
    public float moveForce = 10f;
    public float reachDistance = 0.5f;
    public float maxSpeed = 5f;

    [Header("Climbing")]
    public float climbDeltaY = 0.25f; // treat a segment as 'climb' when the next waypoint differs in Y by this much
    public float ladderDetectRadius = 0.4f;

    private Rigidbody rb;
    private List<Vector3> worldPath;
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    public bool isActiveMoving => isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!astar) astar = FindFirstObjectByType<AStar3D>();
    }

    public void CalculatePath()
    {
        if (!astar) { Debug.LogWarning("No AStar3D reference."); return; }
        if (!astar.grid) astar.grid = FindFirstObjectByType<Grid3D>();
        if (goal == null) { Debug.LogWarning("No goal assigned."); return; }

        // assign self + goal then request a path
        astar.start  = transform;
        astar.target = goal;
        astar.RunAStarImmediately();

        worldPath = astar.GetWorldPath();
        currentWaypointIndex = 0;

        if (worldPath == null || worldPath.Count == 0)
        {
            Debug.Log("No path found.");
            return;
        }
    }

    public void StartPathFollowing()
    {
        CalculatePath(); 
        if (worldPath != null && worldPath.Count > 0)
        {
            isMoving = true;
            currentWaypointIndex = 0;
        }
    }

    public void StopMoving()
    {
        isMoving = false;
        // restore gravity when stopping
        if (rb) rb.useGravity = true;
    }

    private void FixedUpdate()
    {
        if (!isMoving || worldPath == null || worldPath.Count == 0) return;
        if (currentWaypointIndex >= worldPath.Count) { StopMoving(); return; }

        Vector3 targetPosition = worldPath[currentWaypointIndex];
        Vector3 toTarget = targetPosition - transform.position;
        float distanceToWaypoint = toTarget.magnitude;
        if (distanceToWaypoint < reachDistance)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= worldPath.Count) { StopMoving(); return; }
            targetPosition = worldPath[currentWaypointIndex];
            toTarget = targetPosition - transform.position;
            distanceToWaypoint = toTarget.magnitude;
        }

        // decide if this segment is to be climbed
        bool verticalSegment = Mathf.Abs(toTarget.y) >= climbDeltaY;
        bool insideLadder = IsInsideLadder();

        // disable gravity to move vertically
        rb.useGravity = !(verticalSegment || insideLadder);
        Vector3 dir = (distanceToWaypoint > 0.0001f) ? (toTarget / distanceToWaypoint) : Vector3.zero;
        rb.AddForce(dir * moveForce, ForceMode.Acceleration);
        if (rb.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // face move direction (keep upright)
        Vector3 flat = new Vector3(dir.x, 0f, dir.z);
        if (flat.sqrMagnitude > 0.0001f)
        {
            Quaternion face = Quaternion.LookRotation(flat, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, face, Time.fixedDeltaTime * 5f));
        }
        UpdateWaypointProgress(transform.position, reachDistance);
    }

    private void OnDrawGizmos()
    {
        if (worldPath != null && worldPath.Count > 0)
        {
            Gizmos.color = Color.magenta;
            for (int i = 0; i < worldPath.Count - 1; i++)
            {
                Vector3 p1 = worldPath[i];   p1.y += 0.5f;
                Vector3 p2 = worldPath[i+1]; p2.y += 0.5f;
                Gizmos.DrawLine(p1, p2);
                Gizmos.DrawWireSphere(worldPath[i] + Vector3.up * 0.5f, 0.2f);
            }

            Vector3 lastPos = worldPath[worldPath.Count - 1] + Vector3.up * 0.5f;
            Gizmos.DrawWireSphere(lastPos, 0.2f);

            if (isMoving && currentWaypointIndex < worldPath.Count)
            {
                Gizmos.color = Color.yellow;
                Vector3 currentTarget = worldPath[currentWaypointIndex] + Vector3.up * 0.5f;
                Gizmos.DrawWireSphere(currentTarget, reachDistance);
            }
        }
    }

    // 3d check for waypoint progresses
    public bool UpdateWaypointProgress(Vector3 currentPosition, float threshold)
    {
        if (worldPath == null || worldPath.Count == 0 || currentWaypointIndex >= worldPath.Count)
            return false;

        Vector3 toWaypoint = worldPath[currentWaypointIndex] - currentPosition;
        if (toWaypoint.magnitude < threshold)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= worldPath.Count)
            {
                StopMoving();
                return false; // reached end
            }
            return true; // advanced to next
        }
        return true; // still going to current
    }
    private bool IsInsideLadder()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ladderDetectRadius, ~0, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hits.Length; i++)
        {
            if (!hits[i]) continue;
            if (hits[i].GetComponent<Ladder>() != null) return true;
        }
        return false;
    }
}
