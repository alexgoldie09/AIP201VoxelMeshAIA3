using UnityEngine;

/// <summary>
/// A 3D grid of voxels representing a volumetric sampling of space within a navmesh cell.
/// Provides utilities to convert between grid indices and world space, and to query voxel neighbors.
/// </summary>
public class VoxelGrid
{
    public Voxel[,,] voxels;              // 3D array storing all voxels in the grid
    public Vector3Int dimensions;         // Size of the grid in voxels (x, y, z)
    public float voxelSize;               // Length of one side of each voxel cube
    public Vector3 origin;                // Bottom-front-left corner of the voxel grid in world space

    /// <summary>
    /// Initializes the voxel grid based on dimension, voxel size, and world-space origin.
    /// </summary>
    public VoxelGrid(Vector3Int dimensions, float voxelSize, Vector3 origin)
    {
        this.dimensions = dimensions;
        this.voxelSize = voxelSize;
        this.origin = origin;
        this.voxels = new Voxel[dimensions.x, dimensions.y, dimensions.z];
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
    /// Converts a world position to the voxel index within the grid.
    /// </summary>
    public Vector3Int WorldToIndex(Vector3 worldPos)
    {
        Vector3 local = worldPos - origin;
        return new Vector3Int(
            Mathf.FloorToInt(local.x / voxelSize),
            Mathf.FloorToInt(local.y / voxelSize),
            Mathf.FloorToInt(local.z / voxelSize)
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
