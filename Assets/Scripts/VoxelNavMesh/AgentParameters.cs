using UnityEngine;

/// <summary>
/// A serializable class that defines the physical capabilities and restrictions of a navigation agent.
/// Used to determine voxel walkability based on things like size, slope, step height, and water depth.
/// </summary>
[System.Serializable]
public class AgentParameters
{
    [Header("Size & Movement Constraints")]

    [Tooltip("Horizontal radius of the agent, used for clearance checks.")]
    public float radius = 0.4f;

    [Tooltip("Vertical height of the agent, used to verify standing space.")]
    public float height = 2.0f;

    [Tooltip("Maximum height the agent can step onto.")]
    public float maxStepHeight = 0.5f;

    //[Tooltip("Maximum slope angle (in radians) that the agent can traverse.")]
    //public float maxSlopeRad = 0.5f;

    //[Header("Water Navigation")]

    //[Tooltip("Minimum water depth allowed for the agent to walk through.")]
    //public float minWaterDepth = 0.0f;

    //[Tooltip("Maximum water depth the agent can traverse safely.")]
    //public float maxWaterDepth = 1.5f;
}
