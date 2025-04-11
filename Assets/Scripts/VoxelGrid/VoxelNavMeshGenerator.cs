#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VoxelNavMeshGenerator : MonoBehaviour
{
    public Vector3 gridOrigin = Vector3.zero;
    public int gridSizeX = 10;
    public int gridSizeY = 3;
    public int gridSizeZ = 10;
    public float voxelSize = 1f;
    public LayerMask groundMask;
    public float groundCheckHeight = 5f;

    [Header("Mesh Voxel Options")]
    public GameObject voxelPrefab;
    public bool spawnVoxelMeshes = false;

    private VoxelOld[,,] voxelGrid;
    private List<List<Vector3>> extractedPolygons = new List<List<Vector3>>();

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Clear voxels for memory runtime
        ClearVoxels();

        voxelGrid = new VoxelOld[gridSizeX, gridSizeY, gridSizeZ];

        gridOrigin = transform.position;

        // 1. Generate voxels & mark walkable
        GenerateVoxels();

        // 2. Border detection pass using Voxel method
        DetectBorders();

        // 3. Extract polygons
        Extract2DPolygonOutlines(gridSizeY - 1); // Only on base Y level for now

        // 4. Generate navmesh surfaces
        GenerateNavMeshSurfaces();
    }

    private void ClearVoxels()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            List<GameObject> toDestroy = new List<GameObject>();
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("NavMeshSurface") || spawnVoxelMeshes)
                    toDestroy.Add(child.gameObject);
            }

            EditorApplication.delayCall += () =>
            {
                foreach (GameObject obj in toDestroy)
                {
                    if (obj != null)
                        DestroyImmediate(obj);
                }
            };
        }
        else
#endif
        {
            foreach (Transform child in transform)
            {
                if (child.name.StartsWith("NavMeshSurface") || spawnVoxelMeshes)
                    Destroy(child.gameObject);
            }
        }
    }

    private void GenerateVoxels()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    Vector3 worldPos = gridOrigin + new Vector3(x, y, z) * voxelSize;

                    bool isWalkable = Physics.OverlapBox(worldPos, Vector3.one * voxelSize * 0.45f, Quaternion.identity, groundMask).Length > 0;

                    voxelGrid[x, y, z] = new VoxelOld(worldPos, isWalkable);

                    // Mesh spawning (editor/runtime safe)
                    if (spawnVoxelMeshes && voxelPrefab != null)
                    {
                        SpawnVoxelMeshes(x, y, z, worldPos, isWalkable);
                    }

                }
            }
        }
    }

    private void GenerateNavMeshSurfaces()
    {
        // Calculate the topmost Y-level (the highest layer of the grid)
        float topY = 1.01f + (gridSizeY - 1) * voxelSize; // Top of the grid (last row)

        foreach (var polygon in extractedPolygons)
        {
            List<Vector2> flatPoints = polygon.Select(p => new Vector2(p.x, p.z)).ToList();

            // Set the Y coordinate of the navmesh vertices to be at the top of the grid
            List<Vector3> vertices = flatPoints.Select(p => new Vector3(p.x, topY, p.y)).ToList();

            if (!IsConvex(flatPoints))
            {
                Debug.LogWarning("Polygon is not convex. Skipping triangulation.");
                continue;
            }

            flatPoints = SimplifyPolygon(flatPoints, vertices);

            if (CheckSelfIntersection(flatPoints))
            {
                Debug.LogWarning("Polygon has self-intersections. Simplifying using Convex Hull.");
                flatPoints = ConvexHull(flatPoints);
                vertices = flatPoints.Select(p => new Vector3(p.x, topY, p.y)).ToList();  // Ensure Y is still topY
            }

            List<int> indices = PolygonTriangulator.Triangulate(flatPoints);

            if (indices.Count < 3)
            {
                Debug.Log("Triangulation failed. Trying reversed winding...");
                flatPoints.Reverse();
                vertices.Reverse();

                indices = PolygonTriangulator.Triangulate(flatPoints);
            }

            if (indices.Count >= 3)
            {
                CreateNavMeshSurface(vertices, indices);
            }
            else
            {
                Debug.LogWarning("Triangulation failed. Skipping polygon.");
            }
        }
    }


    List<Vector2> SimplifyPolygon(List<Vector2> flatPoints, List<Vector3> vertices)
    {
        float minDistance = 0.1f;
        for (int i = flatPoints.Count - 1; i > 0; i--)
        {
            if (Vector2.Distance(flatPoints[i], flatPoints[i - 1]) < minDistance)
            {
                flatPoints.RemoveAt(i);
                vertices.RemoveAt(i);
            }
        }

        if (flatPoints.Count > 2 && Vector2.Distance(flatPoints[0], flatPoints[flatPoints.Count - 1]) < 0.001f)
        {
            flatPoints.RemoveAt(flatPoints.Count - 1);
            vertices.RemoveAt(vertices.Count - 1);
        }

        return flatPoints;
    }

    private void DetectBorders()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeZ; z++)
                {
                    VoxelOld voxel = voxelGrid[x, y, z];
                    if (voxel != null)
                        voxel.isBorder = voxel.IsBorder(voxelGrid, x, y, z);
                }
            }
        }
    }

    private void SpawnVoxelMeshes(int x, int y, int z, Vector3 worldPos, bool isWalkable)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Delay instantiation until after OnValidate
            Vector3 spawnPos = worldPos;
            bool voxelWalkable = isWalkable;
            bool isBorder = false;

            EditorApplication.delayCall += () =>
            {
                if (this == null || voxelPrefab == null) return;

                GameObject cube = (GameObject)PrefabUtility.InstantiatePrefab(voxelPrefab, this.transform);
                cube.transform.position = spawnPos;
                cube.transform.localScale = Vector3.one * voxelSize; // * 0.9f used for debugging with space;

                Renderer rend = cube.GetComponent<Renderer>();
                if (rend != null)
                {
                    rend.sharedMaterial.color = !voxelWalkable ? Color.red : isBorder ? Color.yellow : Color.green;
                }
            };
        }
        else
#endif
        {
            GameObject cube = Instantiate(voxelPrefab, worldPos, Quaternion.identity, this.transform);
            cube.transform.localScale = Vector3.one * voxelSize;  // * 0.9f used for debugging with space;

            Renderer rend = cube.GetComponent<Renderer>();
            if (rend != null)
            {
                VoxelOld voxel = voxelGrid[x, y, z];
                rend.material.color = !isWalkable ? Color.red : voxel.isBorder ? Color.yellow : Color.green;
            }
        }
    }

    void Extract2DPolygonOutlines(int yLevel)
    {
        extractedPolygons.Clear();

        if (yLevel < 0 || yLevel >= gridSizeY)
        {
            Debug.LogWarning($"[VoxelNavMesh] Invalid yLevel {yLevel}. gridSizeY is {gridSizeY}");
            return;
        }

        bool[,] visited = new bool[gridSizeX, gridSizeZ];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                VoxelOld voxel = voxelGrid[x, yLevel, z];
                if (voxel.isWalkable && voxel.isBorder && !visited[x, z])
                {
                    List<Vector3> polygon = new List<Vector3>();
                    FollowOutline(x, z, yLevel, visited, polygon);
                    if (polygon.Count > 2)
                        extractedPolygons.Add(polygon);
                }
            }
        }

        Debug.Log($"Extracted {extractedPolygons.Count} polygons.");
    }

    void FollowOutline(int startX, int startZ, int y, bool[,] visited, List<Vector3> polygon)
    {
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0), new Vector2Int(0, 1),
            new Vector2Int(-1, 0), new Vector2Int(0, -1)
        };

        int x = startX;
        int z = startZ;
        int dir = 0;

        do
        {
            visited[x, z] = true;
            polygon.Add(voxelGrid[x, y, z].worldPosition);

            for (int i = 0; i < 4; i++)
            {
                int tryDir = (dir + i) % 4;
                int nx = x + directions[tryDir].x;
                int nz = z + directions[tryDir].y;

                if (nx >= 0 && nx < gridSizeX && nz >= 0 && nz < gridSizeZ)
                {
                    if (voxelGrid[nx, y, nz].isWalkable && voxelGrid[nx, y, nz].isBorder && !visited[nx, nz])
                    {
                        x = nx;
                        z = nz;
                        dir = tryDir;
                        break;
                    }
                }
            }

        } while ((x != startX || z != startZ) && polygon.Count < 1000);
    }

    bool CheckSelfIntersection(List<Vector2> points)
    {
        for (int i = 0; i < points.Count; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                if (LinesIntersect(points[i], points[(i + 1) % points.Count], points[j], points[(j + 1) % points.Count]))
                {
                    return true;
                }
            }
        }
        return false;
    }

    bool LinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d = (p1.x - p2.x) * (p3.y - p4.y) - (p1.y - p2.y) * (p3.x - p4.x);
        if (d == 0) return false;

        float pre = (p1.x * p2.y - p1.y * p2.x);
        float post = (p3.x * p4.y - p3.y * p4.x);
        float x = (pre * (p3.x - p4.x) - (p1.x - p2.x) * post) / d;
        float y = (pre * (p3.y - p4.y) - (p1.y - p2.y) * post) / d;

        return (x >= Mathf.Min(p1.x, p2.x) && x <= Mathf.Max(p1.x, p2.x) &&
                x >= Mathf.Min(p3.x, p4.x) && x <= Mathf.Max(p3.x, p4.x) &&
                y >= Mathf.Min(p1.y, p2.y) && y <= Mathf.Max(p1.y, p2.y) &&
                y >= Mathf.Min(p3.y, p4.y) && y <= Mathf.Max(p3.y, p4.y));
    }


    void CreateNavMeshSurface(List<Vector3> polygon, List<int> triangleIndices, string meshName = "NavMeshSurface")
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            List<Vector3> vertsCopy = new List<Vector3>(polygon);
            List<int> trisCopy = new List<int>(triangleIndices);

            EditorApplication.delayCall += () =>
            {
                if (this == null) return;

                GameObject surface = new GameObject(meshName);
                surface.transform.position = Vector3.zero;
                surface.transform.SetParent(this.transform);

                Mesh mesh = new Mesh();
                mesh.name = meshName;
                mesh.SetVertices(vertsCopy);
                mesh.SetTriangles(trisCopy, 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();

                MeshFilter filter = surface.AddComponent<MeshFilter>();
                filter.mesh = mesh;

                MeshRenderer renderer = surface.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = new Material(Shader.Find("Standard"))
                {
                    color = new Color(1f, 0.6f, 0f, 0.5f)
                };
            };
            return;
        }
#endif

        // Regular runtime version
        GameObject go = new GameObject(meshName);
        go.transform.position = Vector3.zero;
        go.transform.SetParent(this.transform);

        Mesh m = new Mesh();
        m.name = meshName;
        m.SetVertices(polygon);
        m.SetTriangles(triangleIndices, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();

        MeshFilter f = go.AddComponent<MeshFilter>();
        f.mesh = m;

        MeshRenderer r = go.AddComponent<MeshRenderer>();
        r.sharedMaterial = new Material(Shader.Find("Standard"))
        {
            color = new Color(1f, 0.6f, 0f, 0.5f)
        };
    }

    void OnValidate()
    {
        // Automatically regenerate the grid in the editor when values change
        GenerateGrid();
    }


    void OnDrawGizmos()
    {
        if (voxelGrid == null) { GenerateGrid(); }

        // Grid outline
        Vector3 gridWorldSize = new Vector3(gridSizeX, gridSizeY, gridSizeZ) * voxelSize;
        Vector3 centerOffset = new Vector3(gridSizeX - 1, gridSizeY - 1, gridSizeZ - 1) * voxelSize * 0.5f;
        Vector3 gridCenter = gridOrigin + centerOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(gridCenter, gridWorldSize);

        // Grid origin marker
        Gizmos.color = Color.cyan;
        Gizmos.DrawCube(gridOrigin, Vector3.one * voxelSize * 0.2f);

        // Draw voxels
        foreach (var voxel in voxelGrid)
        {
            if (voxel == null) continue;

            if (voxel.isBorder)
                Gizmos.color = Color.yellow;
            else if (voxel.isWalkable)
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawWireCube(voxel.worldPosition, Vector3.one * voxelSize);
        }

        // Draw extracted polygon outlines
        foreach (var polygon in extractedPolygons)
        {
            // Convert the Vector3 positions to Vector2 for intersection check
            List<Vector2> flatPolygon = polygon.Select(p => new Vector2(p.x, p.z)).ToList();

            // If the polygon has self-intersections, visualize it in red
            if (CheckSelfIntersection(flatPolygon))
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < polygon.Count; i++)
                {
                    Vector3 p1 = polygon[i] + Vector3.up * 0.1f;
                    Vector3 p2 = polygon[(i + 1) % polygon.Count] + Vector3.up * 0.1f;
                    Gizmos.DrawLine(p1, p2);
                }
            }
            else
            {
                // Otherwise, draw it normally
                Gizmos.color = Color.magenta;
                for (int i = 0; i < polygon.Count; i++)
                {
                    Vector3 p1 = polygon[i] + Vector3.up * 0.1f;
                    Vector3 p2 = polygon[(i + 1) % polygon.Count] + Vector3.up * 0.1f;
                    Gizmos.DrawLine(p1, p2);
                }
            }
        }
    }

    bool IsConvex(List<Vector2> polygon)
    {
        int count = polygon.Count;
        bool isConvex = true;

        for (int i = 0; i < count; i++)
        {
            Vector2 p0 = polygon[i];
            Vector2 p1 = polygon[(i + 1) % count];
            Vector2 p2 = polygon[(i + 2) % count];

            // Calculate the cross product
            float crossProduct = CrossProduct(p0,p1,p2);

            // If cross product is positive, it's a counter-clockwise turn (convex); if negative, it's concave
            if (crossProduct < 0)
            {
                isConvex = false;
                break;
            }
        }

        return isConvex;
    }

    public List<Vector2> ConvexHull(List<Vector2> points)
    {
        // Sort the points
        points = points.OrderBy(p => p.x).ThenBy(p => p.y).ToList();

        // Build lower hull
        List<Vector2> lower = new List<Vector2>();
        foreach (var point in points)
        {
            while (lower.Count >= 2 && CrossProduct(lower[lower.Count - 2], lower[lower.Count - 1], point) <= 0)
            {
                lower.RemoveAt(lower.Count - 1);
            }
            lower.Add(point);
        }

        // Build upper hull
        List<Vector2> upper = new List<Vector2>();
        for (int i = points.Count - 1; i >= 0; i--)
        {
            while (upper.Count >= 2 && CrossProduct(upper[upper.Count - 2], upper[upper.Count - 1], points[i]) <= 0)
            {
                upper.RemoveAt(upper.Count - 1);
            }
            upper.Add(points[i]);
        }

        // Remove the last point of each half because it’s repeated at the beginning of the other half
        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);

        // Combine the two halves
        lower.AddRange(upper);

        return lower;
    }

    private float CrossProduct(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }


}
