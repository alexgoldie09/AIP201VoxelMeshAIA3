/*
 * GAgentVisual.cs
 * ----------------
 * Editor-time helper script that links the GAgent component to this debug visual wrapper.
 *
 * Tasks:
 *  - Runs in both Play and Edit modes (`[ExecuteInEditMode]`).
 *  - On load, finds and stores the attached GAgent reference.
 *
 * Extras:
 *  - You can extend this script to draw Gizmos, goal lines, or status indicators above agents.
 *  - Currently serves as a setup placeholder or a hook for future visualizations.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GAgentVisual : MonoBehaviour
{
    public GAgent thisAgent;

    /*
     * Start()
     * - Assigns the GAgent component attached to this GameObject.
     */
    private void Start()
    {
        thisAgent = GetComponent<GAgent>();
    }
}
