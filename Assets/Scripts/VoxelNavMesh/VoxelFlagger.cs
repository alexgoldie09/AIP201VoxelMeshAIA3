using System.Collections.Generic;
using UnityEngine;

public static class VoxelFlagger
{
    // Tuning parameters
    private const float ClearanceRatioRequired = 0.8f;    // % of agent height that must be clear
    private const float MinSupportDepth = 0.05f;          // How far down support must be

    public static void FlagVoxels(VoxelGrid grid, AgentParameters agent, bool enableDebugLogs, bool enableDebugDraw)
    {
        int walk = 0, non = 0, border = 0;

        for (int x = 0; x < grid.dimensions.x; x++)
        {
            for (int y = 0; y < grid.dimensions.y; y++)
            {
                for (int z = 0; z < grid.dimensions.z; z++)
                {
                    Voxel voxel = grid.voxels[x, y, z];
                    if (voxel == null || voxel.isOccupied)
                    {
                        voxel.type = VoxelType.NonWalkable;
                        non++;
                        continue;
                    }

                    // Measure vertical clearance above
                    int aboveClear = 0;
                    for (int i = 1; i <= Mathf.FloorToInt(agent.height / grid.voxelSize); i++)
                    {
                        Voxel above = grid.Get(x, y + i, z);
                        if (above != null && !above.isOccupied)
                            aboveClear++;
                    }

                    // Measure support depth below
                    int belowSupport = 0;
                    for (int i = 1; i <= Mathf.FloorToInt(agent.maxStepHeight / grid.voxelSize); i++)
                    {
                        Voxel below = grid.Get(x, y - i, z);
                        if (below != null && below.isOccupied)
                            belowSupport++;
                    }

                    float clearanceHeight = aboveClear * grid.voxelSize;
                    float supportDepth = belowSupport * grid.voxelSize;

                    // Debug output
                    // if (enableDebugLogs)
                        // Debug.Log($"Voxel {voxel.position}: ClearanceHeight={clearanceHeight:F2}, SupportDepth={supportDepth:F2}");

                    bool hasEnoughClearance = clearanceHeight >= agent.height * 0.8f;
                    bool hasEnoughSupport = supportDepth >= 0.05f;

                    if (hasEnoughClearance && hasEnoughSupport)
                    {
                        voxel.type = VoxelType.Walkable;
                        walk++;
                    }
                    else
                    {
                        voxel.type = VoxelType.NonWalkable;
                        non++;

                        // Optional draw rays for "almost pass"
                        //if (enableDebugDraw)
                        //{
                        //    if (!hasEnoughClearance && clearanceHeight >= agent.height * 0.6f)
                        //        Debug.DrawRay(voxel.position, Vector3.up * 0.3f, Color.cyan);

                        //    if (!hasEnoughSupport && supportDepth >= 0.025f)
                        //        Debug.DrawRay(voxel.position, Vector3.down * 0.3f, Color.magenta);
                        //}
                    }
                }
            }
        }

        MarkBorders(grid, enableDebugLogs);

        foreach (var v in grid.voxels)
        {
            if (v?.type == VoxelType.Border)
                border++;
        }

        if (enableDebugLogs)
            Debug.Log($"Walkable: {walk}, Border: {border}, NonWalkable: {non}");
    }

    private static void MarkBorders(VoxelGrid grid, bool enableDebugLogs)
    {
        int convertedToBorder = 0;

        for (int x = 0; x < grid.dimensions.x; x++)
        {
            for (int y = 0; y < grid.dimensions.y; y++)
            {
                for (int z = 0; z < grid.dimensions.z; z++)
                {
                    Voxel v = grid.voxels[x, y, z];
                    if (v == null || v.type != VoxelType.Walkable) continue;

                    int nonWalkableNeighbors = 0;

                    foreach (var neighbor in GetAdjacentVoxels(grid, x, y, z))
                    {
                        if (neighbor == null || neighbor.type == VoxelType.NonWalkable)
                            nonWalkableNeighbors++;
                    }

                    if (nonWalkableNeighbors >= 2)
                    {
                        if (enableDebugLogs)
                            Debug.Log($"Voxel at {v.position} changed to Border due to {nonWalkableNeighbors} nonwalkable neighbors.");

                        v.type = VoxelType.Border;
                        convertedToBorder++;
                    }
                }
            }
        }

        if (enableDebugLogs)
            Debug.Log($"Total Walkables converted to Border: {convertedToBorder}");
    }

    private static IEnumerable<Voxel> GetAdjacentVoxels(VoxelGrid grid, int x, int y, int z)
    {
        yield return grid.Get(x + 1, y, z);
        yield return grid.Get(x - 1, y, z);
        yield return grid.Get(x, y + 1, z);
        yield return grid.Get(x, y - 1, z);
        yield return grid.Get(x, y, z + 1);
        yield return grid.Get(x, y, z - 1);
    }
}
