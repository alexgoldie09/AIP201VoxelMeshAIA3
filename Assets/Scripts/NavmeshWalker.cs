using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Moves the agent along a path made of NavMeshNodes, visiting each node's centroid.
/// </summary>
public class NavmeshWalker : MonoBehaviour
{
    public float speed = 2f;

    public NavmeshNode currentNode;
    private Queue<NavmeshNode> path = new();
    private Vector3 targetPos;
    private bool isMoving = false;

    /// <summary>
    /// Sets a new path and starts moving toward the first target node.
    /// </summary>
    public void SetPath(List<NavmeshNode> navPath)
    {
        path = new Queue<NavmeshNode>(navPath);
        if (path.Count > 0)
        {
            isMoving = true;
            AdvanceToNextTarget();
        }
    }

    private void AdvanceToNextTarget()
    {
        if (path.Count > 0)
        {
            currentNode = path.Dequeue();
            targetPos = currentNode.centroid;
        }
        else
        {
            isMoving = false;
            Debug.Log("[NavMeshWalker] Reached final node.");
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
