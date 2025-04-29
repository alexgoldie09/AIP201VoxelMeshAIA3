using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateWorld : MonoBehaviour
{
    public TextMeshProUGUI states;

    private void Start()
    {
        if (states == null)
        {
            states = GetComponent<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
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
