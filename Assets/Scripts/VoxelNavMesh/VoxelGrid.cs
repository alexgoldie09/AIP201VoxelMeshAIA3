using UnityEngine;

/// <summary>
/// A 3D grid of voxels representing a volumetric sampling of space within a navmesh cell.
/// Provides utilities to convert between grid indices and world space, and to query voxel neighbors.
/// </summary>
public class VoxelGrid
{
    public Voxel[,,] voxels;
    public float voxelSize;
    public Vector3 origin;
    public Vector3Int dimensions;

    public VoxelGrid(Voxel[,,] voxels, float voxelSize, Vector3 origin)
    {
        this.voxels = voxels;
        this.voxelSize = voxelSize;
        this.origin = origin;
        this.dimensions = new Vector3Int(
            voxels.GetLength(0),
            voxels.GetLength(1),
            voxels.GetLength(2)
        );
    }

    /// <summary>
    /// Gets the voxel at the specified index, or null if out of bounds.
    /// </summary>
    public Voxel Get(int x, int y, int z)
    {
        if (x < 0 || x >= dimensions.x ||
            y < 0 || y >= dimensions.y ||
            z < 0 || z >= dimensions.z)
            return null;

        return voxels[x, y, z];
    }

    /// <summary>
    /// Converts a voxel index (x, y, z) into world-space center position.
    /// </summary>
    public Vector3 IndexToWorld(int x, int y, int z)
    {
        return origin + new Vector3(
            (x + 0.5f) * voxelSize,
            (y + 0.5f) * voxelSize,
            (z + 0.5f) * voxelSize
        );
    }

    /// <summary>
    /// Converts world position to grid index.
    /// </summary>
    public Vector3Int WorldToIndex(Vector3 worldPosition)
    {
        Vector3 relative = worldPosition - origin + (Vector3.one * voxelSize * 0.5f);
        return new Vector3Int(
            Mathf.FloorToInt(relative.x / voxelSize),
            Mathf.FloorToInt(relative.y / voxelSize),
            Mathf.FloorToInt(relative.z / voxelSize)
        );
    }

    /// <summary>
    /// Checks if the given index is within the grid bounds.
    /// </summary>
    public bool InBounds(int x, int y, int z)
    {
        return x >= 0 && x < dimensions.x &&
               y >= 0 && y < dimensions.y &&
               z >= 0 && z < dimensions.z;
    }

    /// <summary>
    /// 6-directional neighbor offsets (left/right, up/down, forward/backward).
    /// Used for connectivity checks and traversal.
    /// </summary>
    public static readonly Vector3Int[] NeighbourOffsets =
    {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
    };

    /// <summary>
    /// Returns the voxel at the given world position, or null if out of bounds.
    /// </summary>
    public Voxel GetVoxelAtWorldPosition(Vector3 worldPos)
    {
        Vector3Int index = WorldToIndex(worldPos);
        return Get(index.x, index.y, index.z);
    }

}
