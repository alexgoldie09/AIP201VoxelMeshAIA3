using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Classifies voxels in a grid based on agent parameters like height and step ability.
/// Voxels are flagged as Walkable, NonWalkable, or Border depending on clearance, support, and radius fit.
/// </summary>
public static class VoxelFlagger
{
    /// <summary>
    /// Iterates through the voxel grid and assigns a type to each voxel based on agent size and obstacle mask.
    /// </summary>
    public static void FlagVoxels(VoxelGrid grid, AgentParameters agent, bool enableDebugLogs, bool enableDebugDraw)
    {
        const float ClearanceRatioRequired = 0.8f; // Agent must have 80% headroom clearance
        const float MinSupportDepth = 0.05f;       // Must be standing on solid ground (slight leniency)

        for (int x = 0; x < grid.dimensions.x; x++)
        {
            for (int y = 0; y < grid.dimensions.y; y++)
            {
                for (int z = 0; z < grid.dimensions.z; z++)
                {
                    Voxel v = grid.voxels[x, y, z];
                    if (v == null) continue;

                    if (v.isOccupied)
                    {
                        v.type = VoxelType.NonWalkable;
                        continue;
                    }

                    // Count clearance above
                    float clearanceHeight = 0f;
                    for (int cy = y + 1; cy < grid.dimensions.y; cy++)
                    {
                        var above = grid.voxels[x, cy, z];
                        if (above == null || above.isOccupied) break;
                        clearanceHeight += grid.voxelSize;
                        if (clearanceHeight >= agent.height) break;
                    }

                    // Count support below
                    float supportDepth = 0f;
                    for (int sy = y - 1; sy >= 0; sy--)
                    {
                        var below = grid.voxels[x, sy, z];
                        if (below == null || !below.isOccupied) break;
                        supportDepth += grid.voxelSize;
                        if (supportDepth >= agent.maxStepHeight) break;
                    }

                    LayerMask obstacleMask = LayerMask.GetMask("Obstacle");

                    // Check for horizontal fit
                    bool hasHorizontalClearance = !Physics.CheckSphere(
                        v.position,
                        agent.radius,
                        obstacleMask,
                        QueryTriggerInteraction.Ignore
                    );

                    bool hasEnoughClearance = clearanceHeight >= agent.height * ClearanceRatioRequired;
                    bool hasEnoughSupport = supportDepth >= MinSupportDepth;

                    if (hasEnoughClearance && hasEnoughSupport && hasHorizontalClearance)
                        v.type = VoxelType.Walkable;
                    else
                        v.type = VoxelType.NonWalkable;
                }
            }
        }

        // Optional debug logging of Walkable voxel count only (excluding Border)
        if (enableDebugLogs)
        {
            int pureWalkable = 0;
            foreach (var v in grid.voxels)
            {
                if (v == null) continue;
                if (v.type == VoxelType.Walkable) pureWalkable++;
            }
            Debug.Log($"[Debug] Pure Walkable Voxels: {pureWalkable}");
        }

        // Second pass: find edges near drop-offs or obstacles
        MarkBorders(grid);

        if (enableDebugLogs)
        {
            int walkable = 0, border = 0, nonWalkable = 0;
            foreach (var v in grid.voxels)
            {
                if (v == null) continue;
                if (v.type == VoxelType.Walkable) walkable++;
                if (v.type == VoxelType.Border) border++;
                if (v.type == VoxelType.NonWalkable) nonWalkable++;
            }

            Debug.Log($"Walkable: {walkable}, Border: {border}, NonWalkable: {nonWalkable}");
        }
    }

    /// <summary>
    /// Converts walkable voxels into border voxels if they are adjacent to enough non-walkable ones.
    /// Helps define polygon edges later. Less aggressive now (threshold 3+).
    /// </summary>
    private static void MarkBorders(VoxelGrid grid)
    {
        for (int x = 0; x < grid.dimensions.x; x++)
        {
            for (int y = 0; y < grid.dimensions.y; y++)
            {
                for (int z = 0; z < grid.dimensions.z; z++)
                {
                    Voxel v = grid.voxels[x, y, z];
                    if (v == null || v.type != VoxelType.Walkable) continue;

                    int nonWalkableNeighbors = 0;
                    foreach (var offset in VoxelGrid.NeighbourOffsets)
                    {
                        int nx = x + offset.x;
                        int ny = y + offset.y;
                        int nz = z + offset.z;

                        if (!grid.InBounds(nx, ny, nz)) continue;
                        var neighbor = grid.voxels[nx, ny, nz];
                        if (neighbor == null || neighbor.type == VoxelType.NonWalkable)
                            nonWalkableNeighbors++;
                    }

                    if (nonWalkableNeighbors >= 3) // More strict to reduce over-conversion
                        v.type = VoxelType.Border;
                }
            }
        }
    }
}