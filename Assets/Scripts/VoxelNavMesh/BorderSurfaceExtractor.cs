using System.Collections.Generic;
using UnityEngine;

public static class BorderSurfaceExtractor
{
    public static float groupMergeDistance = 0.35f; // tweak as needed

    public static List<List<Vector3>> ExtractConnectedSurfaces(VoxelGrid grid)
    {
        List<List<Vector3>> connectedSurfaces = new();
        bool[,,] visited = new bool[grid.dimensions.x, grid.dimensions.y, grid.dimensions.z];

        for (int y = 0; y < grid.dimensions.y; y++)
        {
            for (int x = 0; x < grid.dimensions.x; x++)
            {
                for (int z = 0; z < grid.dimensions.z; z++)
                {
                    var voxel = grid.voxels[x, y, z];
                    if (voxel == null || voxel.type != VoxelType.Border || visited[x, y, z])
                        continue;

                    List<Vector3> group = new();
                    Queue<Vector2Int> queue = new();
                    queue.Enqueue(new Vector2Int(x, z));
                    visited[x, y, z] = true;

                    while (queue.Count > 0)
                    {
                        Vector2Int current = queue.Dequeue();
                        var curVoxel = grid.voxels[current.x, y, current.y];
                        group.Add(curVoxel.position);

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

    private static readonly Vector2Int[] XZOffsets = new Vector2Int[]
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(-1, 1),
        new Vector2Int(1, -1), new Vector2Int(-1, -1)
    };
}