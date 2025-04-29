using UnityEngine;

public class CookTop : MonoBehaviour
{
    public bool isOccupied = false; // Tracks if cooktop is currently being used

    private void Start()
    {
        // Auto-register this cooktop into GWorld when scene starts
        GWorld.Instance.AddCookTop(this.gameObject);
        GWorld.Instance.GetWorld().ModifyState("Free_CookTop", 1);
    }
}