using UnityEngine;

[System.Serializable]
public class AgentParameters
{
    public float radius = 0.4f;
    public float height = 2.0f;
    public float maxStepHeight = 0.5f;
    public float maxSlopeRad = 0.5f;
    public float minWaterDepth = 0.0f;
    public float maxWaterDepth = 1.5f;
}
