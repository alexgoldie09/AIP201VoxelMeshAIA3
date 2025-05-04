/*
 * GAgentEditor.cs
 * ----------------
 * This class is a custom inspector for the GAgentVisual.cs MonoBehaviour.
 *
 * Tasks:
 *  - Draws detailed GOAP agent data (current action, all actions, goals).
 *  - Provides a developer-friendly view inside the Unity Editor inspector.
 *  - Uses GAgentVisual as the entry point, pulling data from the underlying GAgent.
 *
 * Extras:
 *  - This is editor-only code.
 *  - Visualizes goals and preconditions/effects of actions for debugging purposes.
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(GAgentVisual))]
[CanEditMultipleObjects]
public class GAgentVisualEditor : Editor 
{

    /*
     * OnInspectorGUI() overrides the default Unity inspector UI for GAgentVisual.
     * - Fetches the associated GAgent and prints:
     *     - Name of the agent
     *     - Current action (if any)
     *     - Each action's name, preconditions, and effects
     *     - All active goals and their sub-states
     */
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();

        GAgentVisual agent = (GAgentVisual)target;
        GAgent gAgent = agent.gameObject.GetComponent<GAgent>();

        GUILayout.Label("Name: " + agent.name);
        GUILayout.Label("Current Action: " + gAgent.currentAction);
        GUILayout.Label("Actions: ");

        foreach (GAction a in gAgent.actions)
        {
            string pre = "", eff = "";
            foreach (var p in a.preConditions)
                pre += p.Key + ", ";
            foreach (var e in a.afterEffects)
                eff += e.Key + ", ";

            GUILayout.Label($"====  {a.actionName} ({pre}) ({eff})");
        }

        GUILayout.Label("Goals: ");
        foreach (var g in gAgent.goals)
        {
            GUILayout.Label("---:");
            foreach (var sg in g.Key.sGoals)
                GUILayout.Label("=====  " + sg.Key);
        }

        serializedObject.ApplyModifiedProperties();
    }
}