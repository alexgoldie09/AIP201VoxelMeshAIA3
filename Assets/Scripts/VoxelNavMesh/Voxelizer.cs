using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Converts a NavmeshCell into a VoxelGrid by filling its volume with voxel data and determining which voxels are inside obstacles.
/// </summary>
public static class Voxelizer
{
    public static VoxelGrid VoxelizeCell(NavmeshCell cell, float voxelSize, LayerMask obstacleMask)
    {
        Vector3 cellSize = cell.bounds.size;
        Vector3Int dims = new(
            Mathf.CeilToInt(cellSize.x / voxelSize),
            Mathf.CeilToInt(cellSize.y / voxelSize),
            Mathf.CeilToInt(cellSize.z / voxelSize)
        );

        Voxel[,,] voxels = new Voxel[dims.x, dims.y, dims.z];

        Vector3 startCorner = cell.bounds.min + (Vector3.one * voxelSize * 0.5f);

        for (int x = 0; x < dims.x; x++)
        {
            for (int y = 0; y < dims.y; y++)
            {
                for (int z = 0; z < dims.z; z++)
                {
                    Vector3 pos = startCorner + new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);
                    Vector3Int index = Vector3Int.RoundToInt(pos / voxelSize);

                    bool isOccupied = Physics.CheckBox(pos, Vector3.one * (voxelSize * 0.5f), Quaternion.identity, obstacleMask, QueryTriggerInteraction.Ignore);
                    VoxelType type = isOccupied ? VoxelType.NonWalkable : VoxelType.Walkable;

                    voxels[x, y, z] = new Voxel(pos, isOccupied, type, index);
                }
            }
        }

        return new VoxelGrid(voxels, voxelSize, cell.bounds.center);
    }
}
