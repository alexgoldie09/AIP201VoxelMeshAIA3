using System.Collections.Generic;
using UnityEngine;

public static class PolygonSimplifier
{
    /// <summary>
    /// Simplifies a polygon shape using a basic Ramer-Douglas-Peucker-like method
    /// that retains concave features while reducing minor noise.
    /// </summary>
    public static List<Vector2> SimplifyPolygon(List<Vector2> points, float tolerance)
    {
        if (points == null || points.Count < 3)
            return new List<Vector2>(points);

        bool[] keep = new bool[points.Count];
        for (int i = 0; i < points.Count; i++)
            keep[i] = false;

        SimplifySection(points, 0, points.Count - 1, tolerance, keep);

        // Ensure start and end points are kept
        keep[0] = true;
        keep[points.Count - 1] = true;

        List<Vector2> result = new();
        for (int i = 0; i < points.Count; i++)
        {
            if (keep[i])
                result.Add(points[i]);
        }

        return result;
    }

    private static void SimplifySection(List<Vector2> points, int startIndex, int endIndex, float tolerance, bool[] keep)
    {
        if (endIndex <= startIndex + 1)
            return;

        float maxDistance = -1f;
        int index = -1;
        Vector2 a = points[startIndex];
        Vector2 b = points[endIndex];

        for (int i = startIndex + 1; i < endIndex; i++)
        {
            float dist = PerpendicularDistance(points[i], a, b);
            if (dist > maxDistance)
            {
                maxDistance = dist;
                index = i;
            }
        }

        if (maxDistance > tolerance)
        {
            keep[index] = true;
            SimplifySection(points, startIndex, index, tolerance, keep);
            SimplifySection(points, index, endIndex, tolerance, keep);
        }
    }

    private static float PerpendicularDistance(Vector2 p, Vector2 a, Vector2 b)
    {
        if (a == b)
            return Vector2.Distance(p, a);

        Vector2 ab = b - a;
        Vector2 ap = p - a;
        float t = Vector2.Dot(ap, ab) / ab.sqrMagnitude;
        Vector2 projection = a + t * ab;
        return Vector2.Distance(p, projection);
    }
}
