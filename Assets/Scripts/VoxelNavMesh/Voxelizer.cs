using UnityEngine;

/// <summary>
/// Static utility that voxelizes a given NavmeshCell into a VoxelGrid.
/// Each voxel is tested for collisions against obstacles in the scene.
/// </summary>
public static class Voxelizer
{
    /// <summary>
    /// Converts a NavmeshCell's volume into a voxel grid, where each voxel is marked as occupied or empty.
    /// </summary>
    /// <param name="cell">The world-space region to voxelize.</param>
    /// <param name="voxelSize">The side length of each cubic voxel.</param>
    /// <param name="mask">Layer mask used to detect obstacles.</param>
    /// <returns>A populated VoxelGrid object representing space occupancy.</returns>
    public static VoxelGrid VoxelizeCell(NavmeshCell cell, float voxelSize, LayerMask mask)
    {
        // Calculate how many voxels fit along each axis
        Vector3Int voxelCounts = new Vector3Int(
            Mathf.CeilToInt(cell.size.x / voxelSize),
            Mathf.CeilToInt(cell.size.y / voxelSize),
            Mathf.CeilToInt(cell.size.z / voxelSize)
        );

        // Initialize the grid
        VoxelGrid grid = new VoxelGrid(voxelCounts, voxelSize, cell.origin);

        // Loop through every voxel coordinate in the grid
        for (int x = 0; x < voxelCounts.x; x++)
        {
            for (int y = 0; y < voxelCounts.y; y++)
            {
                for (int z = 0; z < voxelCounts.z; z++)
                {
                    // Calculate the world-space center of this voxel
                    Vector3 voxelCenter = cell.origin + new Vector3(
                        (x + 0.5f) * voxelSize,
                        (y + 0.5f) * voxelSize,
                        (z + 0.5f) * voxelSize
                    );

                    // Check if the voxel overlaps any obstacle using a cube collider check
                    bool isOccupied = Physics.CheckBox(
                        voxelCenter,
                        Vector3.one * (voxelSize / 2f),
                        Quaternion.identity,
                        mask,
                        QueryTriggerInteraction.Ignore
                    );

                    // Create and store the voxel in the grid
                    grid.voxels[x, y, z] = new Voxel(voxelCenter, isOccupied);
                }
            }
        }

        return grid;
    }
}
