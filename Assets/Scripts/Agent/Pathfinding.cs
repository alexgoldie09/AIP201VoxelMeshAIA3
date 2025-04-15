using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class containing A* pathfinding logic using voxel data.
/// </summary>
public static class Pathfinding
{
    public static List<Voxel> walkableVoxels = new();
    public static Dictionary<Vector3Int, Voxel> worldVoxelMap = new();
    public static float voxelSize = 0.3f; // Ensure this matches generation settings

    /// <summary>
    /// Finds the shortest path between two world positions using A*.
    /// </summary>
    public static List<Vector3> FindPath(Vector3 startPos, Vector3 goalPos)
    {
        Voxel start = FindClosestVoxel(startPos);
        Voxel goal = FindClosestVoxel(goalPos);

        if (start == null || goal == null)
        {
            Debug.LogWarning("[Pathfinding] Start or goal voxel is null.");
            return null;
        }

        Debug.Log($"[Pathfinding] Start at {start.position}, Goal at {goal.position}");

        var openSet = new PriorityQueue<Voxel>();
        openSet.Enqueue(start, 0);

        Dictionary<Voxel, Voxel> cameFrom = new();
        Dictionary<Voxel, float> gScore = new() { [start] = 0 };
        Dictionary<Voxel, float> fScore = new() { [start] = Heuristic(start, goal) };

        while (openSet.Count > 0)
        {
            Voxel current = openSet.Dequeue();
            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (Voxel neighbor in GetAdjacentVoxels(current))
            {
                if (neighbor.type == VoxelType.NonWalkable) continue;

                float tentativeG = gScore[current] + 1f;
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goal);

                    if (!openSet.Contains(neighbor))
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                }
            }
        }

        Debug.LogWarning("[Pathfinding] No path found.");
        return null;
    }

    private static Voxel FindClosestVoxel(Vector3 position)
    {
        float bestDist = float.MaxValue;
        Voxel best = null;
        foreach (var voxel in walkableVoxels)
        {
            float dist = Vector3.Distance(position, voxel.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = voxel;
            }
        }

        return best;
    }

    private static float Heuristic(Voxel a, Voxel b)
    {
        return Vector3.Distance(a.position, b.position);
    }

    private static List<Vector3> ReconstructPath(Dictionary<Voxel, Voxel> cameFrom, Voxel current)
    {
        List<Vector3> path = new();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current.position);
            current = cameFrom[current];
        }
        path.Add(current.position);
        path.Reverse();
        return path;
    }

    private static List<Voxel> GetAdjacentVoxels(Voxel voxel)
    {
        List<Voxel> neighbors = new();
        Vector3Int[] offsets = {
            Vector3Int.left, Vector3Int.right,
            Vector3Int.forward, Vector3Int.back,
            Vector3Int.up, Vector3Int.down
        };

        Vector3Int currentIndex = Vector3Int.RoundToInt(voxel.position / voxelSize);

        foreach (var offset in offsets)
        {
            Vector3Int neighborIndex = currentIndex + offset;
            if (worldVoxelMap.TryGetValue(neighborIndex, out Voxel neighbor))
            {
                neighbors.Add(neighbor);
            }
        }

        return neighbors;
    }

    public static List<Vector3> SmoothPath(List<Vector3> rawPath, LayerMask obstacleMask, float agentRadius, float agentHeight)
    {
        if (rawPath == null || rawPath.Count <= 2)
            return rawPath;

        List<Vector3> smoothed = new();
        int current = 0;

        Vector3 verticalOffset = Vector3.up * 0.5f;

        while (current < rawPath.Count - 1)
        {
            int next = rawPath.Count - 1;

            for (int i = rawPath.Count - 1; i > current; i--)
            {
                Vector3 from = rawPath[current];
                Vector3 to = rawPath[i];
                Vector3 direction = (to - from).normalized;
                float distance = Vector3.Distance(from, to);

                Vector3 capsuleStart = from + verticalOffset;
                Vector3 capsuleEnd = capsuleStart + Vector3.up * (agentHeight - 1f);

                if (!Physics.CapsuleCast(
                    capsuleStart,
                    capsuleEnd,
                    agentRadius,
                    direction,
                    out _,
                    distance,
                    obstacleMask))
                {
                    next = i;
                    break;
                }
            }

            smoothed.Add(rawPath[current]);
            current = next;
        }

        smoothed.Add(rawPath[^1]); // Always add final target
        return smoothed;
    }

}
