using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
public class NavmeshDebugMesh : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter filter;
    private MeshRenderer meshRenderer;

    public void BuildMesh(List<Vector2> polygon, List<int> triangles, float y)
    {
        if (polygon == null || triangles == null || polygon.Count < 3 || triangles.Count < 3)
            return;

        // Convert Vector2 to Vector3
        Vector3[] verts = new Vector3[polygon.Count];
        for (int i = 0; i < polygon.Count; i++)
            verts[i] = new Vector3(polygon[i].x, y, polygon[i].y);

        // Create mesh
        mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        if (!TryGetComponent(out filter))
            filter = gameObject.AddComponent<MeshFilter>();
        if (!TryGetComponent(out meshRenderer))
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

        filter.sharedMesh = mesh;

        // Use transparent orange material
        meshRenderer.sharedMaterial = new Material(Shader.Find("Unlit/Color"))
        {
            color = Color.green // semi-transparent orange
        };

        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    private void OnDestroy()
    {
        if (Application.isPlaying) return;
        if (mesh != null)
        {
            DestroyImmediate(mesh);
        }
    }
}
#endif
