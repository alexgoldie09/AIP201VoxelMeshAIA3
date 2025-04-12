using UnityEngine;

/// <summary>
/// Represents a spatial segment (cell) in the navmesh generation grid.
/// Each cell defines a region of the world that will be voxelized and processed independently.
/// </summary>
public class NavmeshCell
{
    public Vector3 origin;      // Bottom-front-left corner of the cell in world space
    public Vector3 size;        // Dimensions of the cell (width, height, depth)
    public Bounds bounds;       // Unity Bounds object used for easy collision checks and containment

    /// <summary>
    /// Initializes a navmesh cell at a given origin with a specific size.
    /// </summary>
    /// <param name="origin">The world-space position of the cell's origin.</param>
    /// <param name="size">The dimensions of the cell in world units.</param>
    public NavmeshCell(Vector3 origin, Vector3 size)
    {
        this.origin = origin;
        this.size = size;

        // Calculate bounds from center + size (Bounds is axis-aligned bounding box)
        this.bounds = new Bounds(origin + size / 2f, size);
    }
}