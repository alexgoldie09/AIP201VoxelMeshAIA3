using UnityEngine;

public static class Voxelizer
{
    public static VoxelGrid VoxelizeCell(NavmeshCell cell, float voxelSize, LayerMask mask)
    {
        Vector3Int voxelCounts = new Vector3Int(
            Mathf.CeilToInt(cell.size.x / voxelSize),
            Mathf.CeilToInt(cell.size.y / voxelSize),
            Mathf.CeilToInt(cell.size.z / voxelSize)
        );

        VoxelGrid grid = new VoxelGrid(voxelCounts, voxelSize, cell.origin);

        for (int x = 0; x < voxelCounts.x; x++)
        {
            for (int y = 0; y < voxelCounts.y; y++)
            {
                for (int z = 0; z < voxelCounts.z; z++)
                {
                    Vector3 voxelCenter = cell.origin + new Vector3(
                        (x + 0.5f) * voxelSize,
                        (y + 0.5f) * voxelSize,
                        (z + 0.5f) * voxelSize
                    );

                    bool isOccupied = Physics.CheckBox(
                        voxelCenter,
                        Vector3.one * (voxelSize / 2f),
                        Quaternion.identity,
                        mask,
                        QueryTriggerInteraction.Ignore
                    );

                    grid.voxels[x, y, z] = new Voxel(voxelCenter, isOccupied);
                }
            }
        }

        return grid;
    }
}
