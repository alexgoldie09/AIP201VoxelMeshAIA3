using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Computes the convex hull of a set of 2D points using Graham scan.
/// </summary>
public static class ConvexHullCalculator
{
    /// <summary>
    /// Returns a new list of points forming the convex hull of the input set.
    /// </summary>
    /// <param name="points">List of 2D points to wrap in a convex shape.</param>
    /// <returns>List of Vector2 forming the convex hull, ordered clockwise.</returns>
    public static List<Vector2> Compute(List<Vector2> points)
    {
        if (points.Count <= 3)
            return new List<Vector2>(points); // Already convex or degenerate

        // 1. Find the lowest Y (and leftmost X in case of tie) to serve as pivot
        points.Sort((a, b) =>
        {
            if (a.y != b.y) return a.y.CompareTo(b.y);
            return a.x.CompareTo(b.x);
        });

        Vector2 origin = points[0]; // Pivot for polar sort

        // 2. Sort remaining points by polar angle relative to the pivot
        points.Sort(1, points.Count - 1, new PolarAngleComparer(origin));

        // 3. Initialize stack with first two points
        Stack<Vector2> stack = new();
        stack.Push(origin);
        stack.Push(points[1]);

        // 4. Graham scan: maintain a convex frontier by removing right-turns
        for (int i = 2; i < points.Count; i++)
        {
            Vector2 top = stack.Pop();

            // Remove any points that make a right turn (cross product <= 0)
            while (stack.Count > 0 && Cross(stack.Peek(), top, points[i]) <= 0)
                top = stack.Pop();

            stack.Push(top);
            stack.Push(points[i]);
        }

        return new List<Vector2>(stack);
    }

    /// <summary>
    /// Computes the cross product of vectors AB and BC.
    /// > 0: Left turn, < 0: Right turn, = 0: Collinear
    /// </summary>
    private static float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    /// <summary>
    /// Custom comparer to sort points by polar angle with respect to a pivot point.
    /// </summary>
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
