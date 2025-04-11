using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ConcaveHullCalculator
{
    /// <summary>
    /// Computes a concave hull for a set of 2D points using a k-nearest neighbor approach.
    /// </summary>
    /// <param name="points">The input list of 2D points.</param>
    /// <param name="k">The k parameter (minimum 3) controlling the level of detail.</param>
    /// <returns>A list of points representing the concave hull polygon, in order.</returns>
    public static List<Vector2> ComputeConcaveHull(List<Vector2> points, int k)
    {
        if (points == null)
            throw new ArgumentNullException(nameof(points));

        if (points.Count < 3)
            return new List<Vector2>(points);

        // Ensure k is at least 3 and not greater than the point count.
        k = Math.Max(3, Math.Min(k, points.Count));

        // Create a modifiable copy of the points.
        List<Vector2> pointSet = new List<Vector2>(points);

        // The resulting concave hull.
        List<Vector2> hull = new List<Vector2>();

        // 1. Find the starting point: the point with the lowest Y 
        // (if tie, the leftmost X) and add it to the hull.
        Vector2 firstPoint = pointSet.OrderBy(p => p.y).ThenBy(p => p.x).First();
        hull.Add(firstPoint);
        pointSet.Remove(firstPoint);

        Vector2 currentPoint = firstPoint;
        // We start with an initial direction pointing directly to the right (0°).
        float previousAngle = 0f;

        bool finished = false;
        while (!finished)
        {
            // 2. Get the k nearest neighbors to the current point.
            List<Vector2> kNearest = GetKNearestNeighbors(pointSet, currentPoint, k);

            // Next candidate point and best (smallest) angle difference.
            bool candidateFound = false;
            Vector2 bestCandidate = Vector2.zero;
            float smallestAngleDiff = 360f;

            // 3. Evaluate each candidate.
            foreach (Vector2 candidate in kNearest)
            {
                // Calculate the angle from the current point to the candidate.
                float angle = GetAngle(currentPoint, candidate);
                // Compute the counter-clockwise difference from our previous edge.
                float angleDiff = (angle - previousAngle + 360f) % 360f;

                // We want the candidate with the smallest positive turning angle.
                if (angleDiff < smallestAngleDiff)
                {
                    // Check that this edge does not intersect any existing edge of the hull.
                    if (!DoesEdgeIntersectHull(hull, currentPoint, candidate))
                    {
                        smallestAngleDiff = angleDiff;
                        bestCandidate = candidate;
                        candidateFound = true;
                    }
                }
            }

            // If no valid candidate is found, increase k (up to the size of the remaining points).
            if (!candidateFound)
            {
                k = Math.Min(pointSet.Count, k + 1);
                // If k has grown too large relative to the remaining points, exit the loop.
                if (k > pointSet.Count + 1)
                    break;
                continue;
            }

            // If the chosen candidate is the starting point, then we can close the hull.
            if (bestCandidate == firstPoint)
            {
                hull.Add(firstPoint);
                finished = true;
            }
            else
            {
                hull.Add(bestCandidate);
                previousAngle = GetAngle(currentPoint, bestCandidate);
                currentPoint = bestCandidate;
                pointSet.Remove(bestCandidate);

                // If there are no more points to consider, close the hull.
                if (pointSet.Count == 0)
                {
                    hull.Add(firstPoint);
                    finished = true;
                }
            }

            // Optional: if the hull becomes self-intersecting or fails a sanity check,
            // you might want to adjust k or otherwise handle this case.
        }

        return hull;
    }

    /// <summary>
    /// Returns the k nearest neighbors of a reference point from a list of points.
    /// </summary>
    private static List<Vector2> GetKNearestNeighbors(List<Vector2> pointSet, Vector2 reference, int k)
    {
        return pointSet.OrderBy(p => Vector2.Distance(reference, p)).Take(k).ToList();
    }

    /// <summary>
    /// Computes the angle in degrees from 'from' to 'to', relative to the positive x-axis.
    /// </summary>
    private static float GetAngle(Vector2 from, Vector2 to)
    {
        return Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
    }

    /// <summary>
    /// Checks whether the line segment between p1 and p2 would intersect any existing edge in the hull.
    /// </summary>
    private static bool DoesEdgeIntersectHull(List<Vector2> hull, Vector2 p1, Vector2 p2)
    {
        // If less than 2 points, there are no edges to check.
        if (hull.Count < 2)
            return false;

        // Check intersection with all existing edges.
        for (int i = 0; i < hull.Count - 1; i++)
        {
            if (LinesIntersect(hull[i], hull[i + 1], p1, p2))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Determines if the two line segments p1-p2 and p3-p4 intersect.
    /// </summary>
    private static bool LinesIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
    {
        float d1 = Direction(p3, p4, p1);
        float d2 = Direction(p3, p4, p2);
        float d3 = Direction(p1, p2, p3);
        float d4 = Direction(p1, p2, p4);

        if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
            ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
            return true;

        // Check for collinearity.
        if (d1 == 0 && OnSegment(p3, p4, p1)) return true;
        if (d2 == 0 && OnSegment(p3, p4, p2)) return true;
        if (d3 == 0 && OnSegment(p1, p2, p3)) return true;
        if (d4 == 0 && OnSegment(p1, p2, p4)) return true;

        return false;
    }

    /// <summary>
    /// Helper function to compute the cross product direction.
    /// </summary>
    private static float Direction(Vector2 a, Vector2 b, Vector2 c)
    {
        return (c.x - a.x) * (b.y - a.y) - (b.x - a.x) * (c.y - a.y);
    }

    /// <summary>
    /// Checks if point c lies on the line segment ab.
    /// </summary>
    private static bool OnSegment(Vector2 a, Vector2 b, Vector2 c)
    {
        return c.x <= Mathf.Max(a.x, b.x) && c.x >= Mathf.Min(a.x, b.x) &&
               c.y <= Mathf.Max(a.y, b.y) && c.y >= Mathf.Min(a.y, b.y);
    }
}
