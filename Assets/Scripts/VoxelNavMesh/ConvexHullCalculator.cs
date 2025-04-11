using System.Collections.Generic;
using UnityEngine;

public static class ConvexHullCalculator
{
    /// <summary>
    /// Computes the convex hull from a list of 2D points using Graham Scan.
    /// </summary>
    public static List<Vector2> Compute(List<Vector2> points)
    {
        if (points.Count <= 3)
            return new List<Vector2>(points); // already a polygon

        // 1. Find the point with the lowest Y (and leftmost X if tied)
        points.Sort((a, b) =>
        {
            if (a.y != b.y) return a.y.CompareTo(b.y);
            return a.x.CompareTo(b.x);
        });

        Vector2 origin = points[0];

        // 2. Sort points by polar angle from origin
        points.Sort(1, points.Count - 1, new PolarAngleComparer(origin));

        // 3. Graham scan stack
        Stack<Vector2> stack = new();
        stack.Push(origin);
        stack.Push(points[1]);

        for (int i = 2; i < points.Count; i++)
        {
            Vector2 top = stack.Pop();
            while (stack.Count > 0 && Cross(stack.Peek(), top, points[i]) <= 0)
                top = stack.Pop();

            stack.Push(top);
            stack.Push(points[i]);
        }

        return new List<Vector2>(stack);
    }

    // > 0 = left turn, < 0 = right turn, 0 = collinear
    private static float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    private class PolarAngleComparer : IComparer<Vector2>
    {
        private readonly Vector2 origin;
        public PolarAngleComparer(Vector2 origin) => this.origin = origin;

        public int Compare(Vector2 a, Vector2 b)
        {
            float angleA = Mathf.Atan2(a.y - origin.y, a.x - origin.x);
            float angleB = Mathf.Atan2(b.y - origin.y, b.x - origin.x);
            return angleA.CompareTo(angleB);
        }
    }
}
