using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Menu Items (ID → Food Name)")]
    public SerializableDictionary<int, string> menu = new SerializableDictionary<int, string>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public KeyValuePair<int, string> GetRandomMenuItem()
    {
        if (menu == null || menu.Count() == 0)
        {
            Debug.LogWarning("[MenuManager] Menu is empty!");
            return default;
        }

        List<int> keys = new List<int>();

        foreach (var pair in menu)
        {
            keys.Add(pair.Key);
        }

        if (keys.Count == 0)
        {
            Debug.LogWarning("[MenuManager] No keys found in menu.");
            return default;
        }

        int randomIndex = Random.Range(0, keys.Count);
        int selectedKey = keys[randomIndex];

        if (menu.TryGetValue(selectedKey, out string foodName))
        {
            return new KeyValuePair<int, string>(selectedKey, foodName);
        }
        else
        {
            Debug.LogWarning($"[MenuManager] No menu item found at key {selectedKey}");
            return default;
        }
    }


    public string GetMenuItemName(int id)
    {
        if (menu.TryGetValue(id, out string name))
        {
            return name;
        }
        return "Unknown";
    }

    public SerializableDictionary<int, string> GetMenu()
    {
        return menu;
    }
}
