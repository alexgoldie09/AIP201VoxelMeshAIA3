using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a triangle node on the navmesh used for pathfinding.
/// Stores triangle vertices and the triangle center (centroid).
/// </summary>
public class NavmeshNode
{
    public Vector3[] vertices = new Vector3[3]; // Vertices of the triangle
    public Vector3 centroid;                   // Midpoint used for navigation or path sampling
    public List<NavmeshNode> neighbors = new(); // List of directly connected triangle nodes

    public NavmeshNode(Vector3 a, Vector3 b, Vector3 c)
    {
        vertices[0] = a;
        vertices[1] = b;
        vertices[2] = c;
        centroid = (a + b + c) / 3f;
    }
}
