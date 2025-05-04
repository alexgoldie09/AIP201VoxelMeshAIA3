/*
 * UpdateWorld.cs
 * --------------
 * This class displays the current world state from GWorld in a UI text element for live debugging.
 *
 * Tasks:
 *  - Accesses the GOAP global world state each frame.
 *  - Iterates through all live state key-value pairs.
 *  - Updates a TextMeshProUGUI element with readable state information.
 *
 * Extras:
 *  - Attach this script to a TextMeshProUGUI object in your scene.
 *  - Helpful for testing GOAP dynamics and verifying planner state transitions.
 */

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateWorld : MonoBehaviour
{
    public TextMeshProUGUI states;

    /*
     * Start()
     * - Attempts to auto-assign the TextMeshProUGUI if not already set in Inspector
     */
    private void Start()
    {
        if (states == null)
        {
            states = GetComponent<TextMeshProUGUI>();
        }
    }

    /*
     * LateUpdate()
     * - Called once per frame (after Update)
     * - Pulls the current world state from GWorld and displays it in formatted lines
     */
    private void LateUpdate()
    {
        var worldStates = GWorld.Instance.GetWorld().GetStates();

        states.text = "";

        foreach (var s in worldStates.GetLivePairs())
        {
            states.text += $"{s.Key.ToUpper()}, {s.Value}\n";
        }
    }

}
