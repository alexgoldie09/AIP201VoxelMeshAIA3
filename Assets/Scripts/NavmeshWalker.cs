using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Moves the agent along a path made of either NavMeshNodes or voxel positions.
/// </summary>
public class NavmeshWalker : MonoBehaviour
{
    public float speed = 2f;

    public NavmeshNode currentNode;
    private Queue<Vector3> voxelPath = new();
    private Vector3 targetPos;
    private bool isMoving = false;
    private float verticalOffset = 0.5f; // Will be initialized in Awake()

    private void Awake()
    {
        // Try to use the agent's height or collider to calculate offset
        if (TryGetComponent(out Collider col))
        {
            verticalOffset = col.bounds.extents.y * 0.5f;
        }
    }

    /// <summary>
    /// Sets a path using triangle-based NavMeshNodes.
    /// </summary>
    public void SetPath(List<NavmeshNode> navPath)
    {
        voxelPath.Clear();
        foreach (var node in navPath)
        {
            voxelPath.Enqueue(node.centroid);
        }

        if (voxelPath.Count > 0)
        {
            isMoving = true;
            AdvanceToNextTarget();
        }
    }

    /// <summary>
    /// Sets a path using raw world-space positions (e.g., walkable voxels).
    /// </summary>
    public void SetPathFromPositions(List<Vector3> positions)
    {
        voxelPath = new Queue<Vector3>(positions);
        if (voxelPath.Count > 0)
        {
            isMoving = true;
            AdvanceToNextTarget();
        }
    }

    private void AdvanceToNextTarget()
    {
        if (voxelPath.Count > 0)
        {
            targetPos = voxelPath.Dequeue() + Vector3.up * verticalOffset;
        }
        else
        {
            isMoving = false;
            Debug.Log("[NavMeshWalker] Reached final voxel.");
        }
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.05f)
        {
            AdvanceToNextTarget();
        }
    }
}