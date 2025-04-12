using UnityEngine;

/// <summary>
/// Defines the walkability state of a voxel.
/// </summary>
public enum VoxelType
{
    Unknown,        // Not yet classified
    Walkable,       // Safe for agent to walk on
    NonWalkable,    // Blocked by an obstacle
    Border          // Edge between walkable and non-walkable
}

/// <summary>
/// Represents a single voxel within the voxel grid structure.
/// Stores its world position, occupancy status, and classified type.
/// </summary>
public class Voxel
{
    public Vector3 position;       // World-space center of the voxel
    public bool isOccupied;        // True if the voxel overlaps any obstacle collider
    public VoxelType type = VoxelType.Unknown; // Initial classification

    /// <summary>
    /// Constructs a voxel at a given position and occupancy status.
    /// </summary>
    /// <param name="position">World-space center position of the voxel.</param>
    /// <param name="isOccupied">Whether the voxel is inside an obstacle.</param>
    public Voxel(Vector3 position, bool isOccupied)
    {
        this.position = position;
        this.isOccupied = isOccupied;
    }
}
