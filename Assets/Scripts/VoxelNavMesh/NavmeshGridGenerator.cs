using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class NavmeshGridGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public Vector3 gridOrigin = Vector3.zero;
    public Vector3 cellSize = new Vector3(5, 5, 5);
    public Vector3Int gridDimensions = new Vector3Int(4, 1, 4);
    [Range(0f, 0.5f)]
    public float overlapPercent = 0.1f;

    [Header("Voxel Settings")]
    public float voxelSize = 0.3f;
    public LayerMask obstacleMask = ~0;
    public AgentParameters agentParameters = new AgentParameters();

    [Header("Debug")]
    public bool enableDebugDraw = true;
    public bool enableDebugLogs = false;
    public bool showGridBounds = true;
    public bool showVoxels = true;
    public bool showSimplifiedPolygons = true;
    public bool showTriangleDebug = true;
    public float simplificationTolerance = 0.3f;

    private List<NavmeshCell> cells = new List<NavmeshCell>();
    private Dictionary<NavmeshCell, VoxelGrid> cellVoxelGrids = new();

#if UNITY_EDITOR
    private List<GameObject> debugMeshObjects = new();

    private void ClearDebugMeshes()
    {
        foreach (var go in debugMeshObjects)
        {
            if (go != null)
                DestroyImmediate(go);
        }
        debugMeshObjects.Clear();
    }
#endif

    /// <summary>
    /// Manual trigger to regenerate everything (in Inspector).
    /// </summary>
    [ContextMenu("Rebuild Navmesh")]
    public void Rebuild()
    {
#if UNITY_EDITOR
        ClearDebugMeshes();
#endif
        GenerateCells();
        VoxelizeCells();
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        // Optional auto-run
        // Rebuild();
#endif
    }

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

    private void OnDrawGizmos()
    {
        if (!enableDebugDraw) return;

#if UNITY_EDITOR
        ClearDebugMeshes();
#endif

        // Draw cell boundaries
        if (showGridBounds)
        {
            Gizmos.color = Color.yellow;
            foreach (var cell in cells)
            {
                Gizmos.DrawWireCube(cell.bounds.center, cell.bounds.size);
            }
        }

        List<Vector2> combined2D = new();
        float targetY = -1f;

        foreach (var kvp in cellVoxelGrids)
        {
            var grid = kvp.Value;

            // ✅ 1. Voxel Debug Gizmos (now optional)
            for (int x = 0; x < grid.dimensions.x; x++)
            {
                for (int y = 0; y < grid.dimensions.y; y++)
                {
                    for (int z = 0; z < grid.dimensions.z; z++)
                    {
                        var voxel = grid.voxels[x, y, z];
                        if (voxel == null) continue;

                        if (voxel.type == VoxelType.Border)
                        {
                            if (targetY < 0) targetY = voxel.position.y;
                            combined2D.Add(new Vector2(voxel.position.x, voxel.position.z));
                        }

                        if (showVoxels)
                        {
                            switch (voxel.type)
                            {
                                case VoxelType.Walkable:
                                    Gizmos.color = Color.green;
                                    break;
                                case VoxelType.Border:
                                    Gizmos.color = Color.cyan;
                                    break;
                                //case VoxelType.NonWalkable:
                                //    Gizmos.color = Color.red;
                                //    break;
                                default:
                                    continue;
                            }
                            Gizmos.DrawWireCube(voxel.position, Vector3.one * voxelSize * 0.95f);
                        }
                    }
                }
            }

            // ✅ Step 2d - Group border voxels and build polygon outlines
            var surfaceGroups = BorderSurfaceExtractor.ExtractConnectedSurfaces(grid);

            foreach (var group in surfaceGroups)
            {
                if (group.Count < 3) continue;

                float y = group[0].y;
                List<Vector2> projected = new();
                foreach (var v in group)
                    projected.Add(new Vector2(v.x, v.z));

                var simplified = ConvexHullCalculator.Compute(projected);

                if (showSimplifiedPolygons)
                {
                    for (int i = 0; i < simplified.Count; i++)
                    {
                        Vector3 a = new Vector3(simplified[i].x, y, simplified[i].y);
                        Vector3 b = new Vector3(simplified[(i + 1) % simplified.Count].x, y, simplified[(i + 1) % simplified.Count].y);
                        Debug.DrawLine(a, b, Color.magenta);
                    }
                }

#if UNITY_EDITOR
                if (showTriangleDebug)
                {
                    var tris = PolygonTriangulator.Triangulate(simplified);
                    GameObject debugMeshGO = new GameObject("DebugNavmeshPatch");
                    debugMeshGO.transform.position = Vector3.zero;
                    debugMeshGO.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;

                    var debugMesh = debugMeshGO.AddComponent<NavmeshDebugMesh>();
                    debugMesh.BuildMesh(simplified, tris, y);
                    debugMeshObjects.Add(debugMeshGO);
                }
#endif
            }
        }
    }
}
