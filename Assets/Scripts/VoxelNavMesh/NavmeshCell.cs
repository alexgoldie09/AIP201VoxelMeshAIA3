using UnityEngine;

public class NavmeshCell
{
    public Vector3 origin;      // World position of the bottom corner
    public Vector3 size;        // Dimensions of the cell
    public Bounds bounds;       // For easy collision checks

    public NavmeshCell(Vector3 origin, Vector3 size)
    {
        this.origin = origin;
        this.size = size;
        this.bounds = new Bounds(origin + size / 2, size);
    }
}