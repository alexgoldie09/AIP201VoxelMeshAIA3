using UnityEngine;

/// <summary>
/// Enum representing the type of voxel based on clearance and surroundings.
/// </summary>
public enum VoxelType
{
    Walkable,
    NonWalkable,
    Border
}

/// <summary>
/// Represents a single voxel in the grid with its world position, occupancy, index, and classification type.
/// </summary>
public class Voxel
{
    public Vector3 position;          // World-space position of the voxel center
    public Vector3Int index;          // Grid index (for faster lookups and adjacency)
    public bool isOccupied;           // Whether this voxel overlaps an obstacle
    public VoxelType type;            // Classification: Walkable, NonWalkable, or Border

    public Voxel(Vector3 position, bool isOccupied, VoxelType type, Vector3Int index)
    {
        this.position = position;
        this.isOccupied = isOccupied;
        this.type = type;
        this.index = index;
    }
}