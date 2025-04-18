﻿using UnityEngine;
using System.Collections.Generic;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Generates a voxel-based navmesh by dividing the world into cells, voxelizing space, flagging walkable regions,
/// extracting and simplifying border polygons, triangulating them, and optionally visualizing the results.
/// </summary>
[ExecuteAlways]
public class NavmeshGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector3 cellSize = new Vector3(5, 5, 5); // Size of each navmesh cell
    public Vector3Int gridDimensions = new Vector3Int(4, 1, 4); // How many cells along each axis
    [Range(0f, 0.5f)]
    public float overlapPercent = 0.1f; // Amount of overlap between cells
    private Vector3 gridOrigin = Vector3.zero; // Starting position of the navmesh grid

    [Header("Voxel Settings")]
    public float voxelSize = 0.3f; // Size of each voxel cube
    public LayerMask obstacleMask = ~0; // Layers considered obstacles
    public AgentParameters agentParameters = new AgentParameters(); // Defines agent's radius and height

    [Header("Debug")]
    public bool enableDebugDraw = true;
    public bool enableDebugLogs = false;
    public bool showGridBounds = true;
    public bool showVoxels = true;
    public bool showSimplifiedPolygons = true;
    public float simplificationTolerance = 0.3f;


    private List<NavmeshCell> cells = new List<NavmeshCell>();
    private Dictionary<NavmeshCell, VoxelGrid> cellVoxelGrids = new();
    private List<int> masterTriangleList = new();
    private List<NavmeshNode> navMeshNodes = new();

    /// <summary>
    /// Rebuilds the entire navmesh: cells, voxel data, and polygon surfaces.
    /// </summary>
    [ContextMenu("Rebuild Navmesh")]
    public void Rebuild()
    {
        GenerateCells();
        VoxelizeCells();
        GenerateNavmeshNodes();
        StoreWalkableVoxels();
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            Rebuild();
        }
    }


    /// <summary>
    /// Creates a grid of navmesh cells with optional overlap.
    /// </summary>
    private void GenerateCells()
    {
        cells.Clear();
        Vector3 overlap = cellSize * overlapPercent;
        gridOrigin = transform.position;

        for (int x = 0; x < gridDimensions.x; x++)
        {
            for (int y = 0; y < gridDimensions.y; y++)
            {
                for (int z = 0; z < gridDimensions.z; z++)
                {
                    Vector3 offset = new Vector3(
                        x * (cellSize.x - overlap.x),
                        y * (cellSize.y - overlap.y),
                        z * (cellSize.z - overlap.z)
                    );

                    Vector3 origin = gridOrigin + offset;
                    cells.Add(new NavmeshCell(origin, cellSize));
                }
            }
        }
    }

    /// <summary>
    /// Voxelizes each cell and flags its voxels using agent clearance checks.
    /// </summary>
    private void VoxelizeCells()
    {
        cellVoxelGrids.Clear();
        foreach (var cell in cells)
        {
            var grid = Voxelizer.VoxelizeCell(cell, voxelSize, obstacleMask);
            VoxelFlagger.FlagVoxels(grid, agentParameters, enableDebugLogs, enableDebugDraw);
            cellVoxelGrids[cell] = grid;
        }
    }

    /// <summary>
    /// Stores all walkable voxels in a static Pathfinding class for global access.
    /// </summary>
    private void StoreWalkableVoxels()
    {
        List<Voxel> walkable = new();
        Dictionary<Vector3Int, Voxel> voxelMap = new();

        Pathfinding.voxelSize = voxelSize;

        foreach (var grid in cellVoxelGrids.Values)
        {
            for (int x = 0; x < grid.dimensions.x; x++)
            {
                for (int y = 0; y < grid.dimensions.y; y++)
                {
                    for (int z = 0; z < grid.dimensions.z; z++)
                    {
                        var voxel = grid.voxels[x, y, z];
                        if (voxel != null && voxel.type == VoxelType.Walkable)
                        {
                            Pathfinding.walkableVoxels.Add(voxel);
                            Pathfinding.worldVoxelMap[voxel.index] = voxel;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws debug visuals for the grid, voxel classification, polygon outlines, and triangle info.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!enableDebugDraw) return;

        if (showGridBounds)
        {
            Gizmos.color = Color.yellow;
            foreach (var cell in cells)
            {
                Gizmos.DrawWireCube(cell.bounds.center, cell.bounds.size);
            }
        }

        foreach (var kvp in cellVoxelGrids)
        {
            var grid = kvp.Value;

            // Draw voxel classification if enabled
            if (showVoxels)
            {
                for (int x = 0; x < grid.dimensions.x; x++)
                {
                    for (int y = 0; y < grid.dimensions.y; y++)
                    {
                        for (int z = 0; z < grid.dimensions.z; z++)
                        {
                            var voxel = grid.voxels[x, y, z];
                            if (voxel == null) continue;

                            switch (voxel.type)
                            {
                                case VoxelType.Walkable:
                                    Gizmos.color = Color.green;
                                    break;
                                case VoxelType.Border:
                                    Gizmos.color = Color.cyan;
                                    break;
                                default:
                                    continue;
                            }
                            Gizmos.DrawWireCube(voxel.position, Vector3.one * voxelSize * 0.95f);
                        }
                    }
                }
            }
        }

        GenerateNavmeshNodes();
    }

    #region **ORIGINAL POLYGON NAVMESH**
    private void GenerateNavmeshNodes()
    {
        float targetY = -1f;
        masterTriangleList.Clear();
        navMeshNodes.Clear();

        foreach (var kvp in cellVoxelGrids)
        {
            var grid = kvp.Value;
            var surfaceGroups = BorderSurfaceExtractor.ExtractConnectedSurfaces(grid);
            foreach (var group in surfaceGroups)
            {
                if (group.Count < 3) continue;
                if (targetY < 0) targetY = grid.voxels[0, 0, 0].position.y;

                var convex = ConvexHullCalculator.Compute(group);
                var simplified = PolygonSimplifier.SimplifyPolygon(convex, simplificationTolerance);

                if (showSimplifiedPolygons)
                {
                    for (int i = 0; i < simplified.Count; i++)
                    {
                        Vector3 a = new Vector3(simplified[i].x, targetY, simplified[i].y);
                        Vector3 b = new Vector3(simplified[(i + 1) % simplified.Count].x, targetY, simplified[(i + 1) % simplified.Count].y);
                        Debug.DrawLine(a, b, Color.magenta);
                    }
                }

                var triangleIndices = PolygonTriangulator.Triangulate(simplified);
                masterTriangleList.AddRange(triangleIndices);

                for (int i = 0; i < triangleIndices.Count; i += 3)
                {
                    Vector2 vA2D = simplified[triangleIndices[i]];
                    Vector2 vB2D = simplified[triangleIndices[i + 1]];
                    Vector2 vC2D = simplified[triangleIndices[i + 2]];

                    Vector3 vA = new Vector3(vA2D.x, targetY, vA2D.y);
                    Vector3 vB = new Vector3(vB2D.x, targetY, vB2D.y);
                    Vector3 vC = new Vector3(vC2D.x, targetY, vC2D.y);

                    NavmeshNode node = new NavmeshNode(vA, vB, vC);
                    navMeshNodes.Add(node);

                   //if (enableDebugLogs)
                   //     Debug.Log($"[NavMeshNode] Created node at {node.centroid}");
                }
            }
        }

        ConnectNavMeshNodes();
    }

    /// <summary>
    /// Compares all triangle nodes and connects those that share an edge.
    /// </summary>
    private void ConnectNavMeshNodes()
    {
        for (int i = 0; i < navMeshNodes.Count; i++)
        {
            NavmeshNode a = navMeshNodes[i];
            for (int j = i + 1; j < navMeshNodes.Count; j++)
            {
                NavmeshNode b = navMeshNodes[j];

                if (ShareEdge(a, b))
                {
                    a.neighbors.Add(b);
                    b.neighbors.Add(a);
                }
            }
        }
    }

    /// <summary>
    /// Determines if two triangles share exactly 2 vertices (a common edge).
    /// </summary>
    private bool ShareEdge(NavmeshNode a, NavmeshNode b)
    {
        int shared = 0;
        foreach (var va in a.vertices)
        {
            foreach (var vb in b.vertices)
            {
                if (Vector3.Distance(va, vb) < 0.3f)
                    shared++;
            }
        }
        return shared == 2;
    }
    #endregion
}