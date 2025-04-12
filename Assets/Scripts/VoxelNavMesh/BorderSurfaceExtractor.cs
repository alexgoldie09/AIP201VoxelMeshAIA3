using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Extracts connected planar border voxel surfaces from a voxel grid.
/// Performs 2D flood fill in the XZ plane at each Y layer.
/// </summary>
public static class BorderSurfaceExtractor
{
    // Maximum distance between border voxels to be grouped together
    public static float groupMergeDistance = 1f;

    /// <summary>
    /// Returns a list of surface groups, each representing a contiguous border patch.
    /// Each surface is a list of Vector2 positions (projected XZ center of each border voxel).
    /// </summary>
    public static List<List<Vector2>> ExtractConnectedSurfaces(VoxelGrid grid)
    {
        List<List<Vector2>> connectedSurfaces = new();
        bool[,,] visited = new bool[grid.dimensions.x, grid.dimensions.y, grid.dimensions.z];

        // Process each Y-layer separately (flat planar navmesh assumption)
        for (int y = 0; y < grid.dimensions.y; y++)
        {
            for (int x = 0; x < grid.dimensions.x; x++)
            {
                for (int z = 0; z < grid.dimensions.z; z++)
                {
                    var voxel = grid.voxels[x, y, z];
                    if (voxel == null || voxel.type != VoxelType.Border || visited[x, y, z])
                        continue;

                    // Start a new flood fill for this group
                    List<Vector2> group = new();
                    Queue<Vector2Int> queue = new();
                    queue.Enqueue(new Vector2Int(x, z));
                    visited[x, y, z] = true;

                    while (queue.Count > 0)
                    {
                        Vector2Int current = queue.Dequeue();
                        var curVoxel = grid.voxels[current.x, y, current.y];
                        group.Add(new Vector2(curVoxel.position.x, curVoxel.position.z));

                        foreach (var offset in XZOffsets)
                        {
                            int nx = current.x + offset.x;
                            int nz = current.y + offset.y;

                            if (!grid.InBounds(nx, y, nz)) continue;
                            if (visited[nx, y, nz]) continue;

                            var neighbor = grid.voxels[nx, y, nz];
                            if (neighbor == null || neighbor.type != VoxelType.Border) continue;

                            if (Vector3.Distance(curVoxel.position, neighbor.position) > groupMergeDistance)
                                continue;

                            visited[nx, y, nz] = true;
                            queue.Enqueue(new Vector2Int(nx, nz));
                        }
                    }

                    if (group.Count >= 3)
                        connectedSurfaces.Add(group);
                }
            }
        }

        return connectedSurfaces;
    }

    /// <summary>
    /// 8-directional offsets in the XZ plane (includes diagonals)
    /// </summary>
    private static readonly Vector2Int[] XZOffsets = new Vector2Int[]
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(-1, 1),
        new Vector2Int(1, -1), new Vector2Int(-1, -1)
    };
}