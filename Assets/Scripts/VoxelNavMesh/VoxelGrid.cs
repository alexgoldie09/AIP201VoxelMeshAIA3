using UnityEngine;

public class VoxelGrid
{
    public Voxel[,,] voxels;              // 3D array of voxels
    public Vector3Int dimensions;         // Number of voxels in x, y, z
    public float voxelSize;              // Side length of each voxel
    public Vector3 origin;               // World position of the grid's bottom-left-front corner

    public VoxelGrid(Vector3Int dimensions, float voxelSize, Vector3 origin)
    {
        this.dimensions = dimensions;
        this.voxelSize = voxelSize;
        this.origin = origin;
        this.voxels = new Voxel[dimensions.x, dimensions.y, dimensions.z];
    }

    // Safely get a voxel at grid index (x, y, z)
    public Voxel Get(int x, int y, int z)
    {
        if (x < 0 || x >= dimensions.x ||
            y < 0 || y >= dimensions.y ||
            z < 0 || z >= dimensions.z)
            return null;

        return voxels[x, y, z];
    }

    // Convert a grid index to world position
    public Vector3 IndexToWorld(int x, int y, int z)
    {
        return origin + new Vector3(
            (x + 0.5f) * voxelSize,
            (y + 0.5f) * voxelSize,
            (z + 0.5f) * voxelSize
        );
    }

    // Optional: Convert a world position to voxel indices
    public Vector3Int WorldToIndex(Vector3 worldPos)
    {
        Vector3 local = worldPos - origin;
        return new Vector3Int(
            Mathf.FloorToInt(local.x / voxelSize),
            Mathf.FloorToInt(local.y / voxelSize),
            Mathf.FloorToInt(local.z / voxelSize)
        );
    }

    public bool InBounds(int x, int y, int z)
    {
        return x >= 0 && x < dimensions.x &&
               y >= 0 && y < dimensions.y &&
               z >= 0 && z < dimensions.z;
    }

    public static readonly Vector3Int[] NeighbourOffsets =
    {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
    };
}
