using UnityEngine;

public enum VoxelType
{
    Unknown,
    Walkable,
    NonWalkable,
    Border
}

public class Voxel
{
    public Vector3 position;
    public bool isOccupied;
    public VoxelType type = VoxelType.Unknown;

    public Voxel(Vector3 position, bool isOccupied)
    {
        this.position = position;
        this.isOccupied = isOccupied;
    }
}
