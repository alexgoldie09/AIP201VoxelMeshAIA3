using System.Collections.Generic;
using UnityEngine;

public static class PolygonTriangulator
{
    public static List<int> Triangulate(List<Vector2> points)
    {
        List<int> indices = new List<int>();

        if (points.Count < 3)
            return indices;

        List<int> verts = new List<int>();
        for (int i = 0; i < points.Count; i++) verts.Add(i);

        while (verts.Count >= 3)
        {
            bool earFound = false;

            for (int i = 0; i < verts.Count; i++)
            {
                int i0 = verts[(i + verts.Count - 1) % verts.Count];
                int i1 = verts[i];
                int i2 = verts[(i + 1) % verts.Count];

                Vector2 a = points[i0];
                Vector2 b = points[i1];
                Vector2 c = points[i2];

                if (IsConvex(a, b, c))
                {
                    bool ear = true;
                    for (int j = 0; j < points.Count; j++)
                    {
                        if (j == i0 || j == i1 || j == i2) continue;
                        if (PointInTriangle(points[j], a, b, c))
                        {
                            ear = false;
                            break;
                        }
                    }

                    if (ear)
                    {
                        indices.Add(i0);
                        indices.Add(i1);
                        indices.Add(i2);
                        verts.RemoveAt(i);
                        earFound = true;
                        break;
                    }
                }
            }

            if (!earFound)
                break; // No more ears, likely a bad polygon
        }

        return indices;
    }

    static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x) < 0;
    }

    static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
        float s = 1f / (2f * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
        float t = 1f / (2f * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
        return s > 0 && t > 0 && (1 - s - t) > 0;
    }
}
