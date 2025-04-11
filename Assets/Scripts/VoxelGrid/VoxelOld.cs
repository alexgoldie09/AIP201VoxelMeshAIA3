using UnityEngine;

public enum AreaType
{
    Walkable,
    NonWalkable,
    Obstacle,
    Custom // For any other area types you might want to define
}


public class VoxelOld
{
    public Vector3 worldPosition;
    public bool isWalkable;
    public bool isBorder;

    public VoxelOld(Vector3 worldPosition, bool isWalkable)
    {
        this.worldPosition = worldPosition;
        this.isWalkable = isWalkable;
        this.isBorder = false;
    }

    public bool IsBorder(VoxelOld[,,] grid, int x, int y, int z)
    {
        int maxX = grid.GetLength(0);
        int maxY = grid.GetLength(1);
        int maxZ = grid.GetLength(2);

        // Only consider walkable voxels
        if (!this.isWalkable)
            return false;

        // Check 6-connected neighbors
        Vector3Int[] offsets = new Vector3Int[]
        {
        new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
        new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
        new Vector3Int(0, 0, 1), new Vector3Int(0, 0, -1)
        };

        foreach (Vector3Int offset in offsets)
        {
            int nx = x + offset.x;
            int ny = y + offset.y;
            int nz = z + offset.z;

            if (nx < 0 || nx >= maxX || ny < 0 || ny >= maxY || nz < 0 || nz >= maxZ)
                return true; // edge of the grid = border

            if (!grid[nx, ny, nz].isWalkable)
                return true; // adjacent to non-walkable = border
        }

        return false;
    }

}