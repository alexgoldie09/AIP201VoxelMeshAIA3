using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Smooth AI movement controller that steers toward smoothed A* voxel path.
/// Includes predictive steering and damped acceleration.
/// </summary>
public class NavmeshWalker : MonoBehaviour
{
    public Transform startTransform;
    public Transform goalTransform;

    [Header("Agent Settings")]
    public float moveSpeed = 2f;
    public float rotationSpeed = 5f;
    public float smoothTime = 0.3f; // Increased to slow down smoothing
    public float waypointSwitchThreshold = 0.5f;
    private float agentRadius = 0.8f;
    private float agentHeight = 2f;

    private List<Vector3> currentPath = new();
    private int currentIndex = 0;
    private bool isMoving = false;
    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule != null)
        {
            agentRadius = capsule.radius + 0.3f;
            agentHeight = capsule.height;
        }
        else
        {
            Debug.LogWarning("[NavmeshWalker] No CapsuleCollider found. Using default values.");
            agentRadius = 0.8f;
            agentHeight = 2f;
        }
        StartCoroutine(WaitForVoxelGridAndRequestPath());
    }

    private IEnumerator WaitForVoxelGridAndRequestPath()
    {
        while (Pathfinding.walkableVoxels.Count == 0)
            yield return null;

        RequestPath();
    }

    private void RequestPath()
    {
        currentPath = Pathfinding.FindPath(startTransform.position, goalTransform.position);
        currentPath = Pathfinding.SmoothPath(currentPath, LayerMask.GetMask("Obstacle"), agentRadius, agentHeight);

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning("[NavmeshWalker] No path found.");
            return;
        }

        // Insert start position as the first waypoint so it blends from current position
        currentPath.Insert(0, transform.position);

        Debug.Log("[NavmeshWalker] Path found with " + currentPath.Count + " points.");
        currentIndex = 0;
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving || currentPath == null || currentIndex >= currentPath.Count)
            return;

        // Predictive steering: look ahead one point
        int lookAheadIndex = Mathf.Clamp(currentIndex + 1, 0, currentPath.Count - 1);
        Vector3 currentTarget = currentPath[lookAheadIndex];
        Vector3 flatTarget = new Vector3(currentTarget.x, transform.position.y, currentTarget.z);
        Vector3 direction = flatTarget - transform.position;
        float distance = direction.magnitude;

        // Smooth movement toward target
        transform.position = Vector3.SmoothDamp(transform.position, flatTarget, ref velocity, smoothTime, moveSpeed);

        // Smooth rotation toward direction
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // Dynamic switching logic to next point
        if (Vector3.Dot(direction.normalized, transform.forward) > 0.95f && distance < waypointSwitchThreshold)
        {
            if (currentIndex < currentPath.Count - 1)
            {
                currentIndex++;
            }
            else
            {
                Debug.Log("[NavmeshWalker] Reached final target.");
                isMoving = false;
            }
        }
    }
}
