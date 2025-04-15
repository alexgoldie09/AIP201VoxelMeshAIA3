using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(WorldState))]
public class WorldStateDrawer : PropertyDrawer
{
    private const float keyWidth = 150f;
    private const float buttonWidth = 20f;

    private static readonly string[] sampleKeys = new[]
    {
        "hasWeapon", "isHungry", "enemyNearby", "inCover", "lowHealth", "goalReached", "isRunning", "isScared", "hasAmmo", "canSeePlayer"
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty listProp = property.FindPropertyRelative("keyValuePairs");

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        if (listProp != null && listProp.isArray)
        {
            float y = position.y;
            HashSet<string> seenKeys = new();

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty pair = listProp.GetArrayElementAtIndex(i);
                SerializedProperty key = pair.FindPropertyRelative("Key");
                SerializedProperty value = pair.FindPropertyRelative("Value");

                Rect keyRect = new Rect(position.x, y, keyWidth, EditorGUIUtility.singleLineHeight);
                Rect valueRect = new Rect(position.x + keyWidth + 5, y, position.width - keyWidth - buttonWidth - 10, EditorGUIUtility.singleLineHeight);
                Rect removeButtonRect = new Rect(position.x + position.width - buttonWidth, y, buttonWidth, EditorGUIUtility.singleLineHeight);

                // Highlight duplicate keys
                bool isDuplicate = !string.IsNullOrEmpty(key.stringValue) && !seenKeys.Add(key.stringValue);
                if (isDuplicate)
                    EditorGUI.DrawRect(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), new Color(1f, 0.5f, 0.5f, 0.25f));

                // Draw key and value
                EditorGUI.PropertyField(keyRect, key, GUIContent.none);
                EditorGUI.PropertyField(valueRect, value, GUIContent.none);

                // Remove button
                if (GUI.Button(removeButtonRect, "–"))
                {
                    listProp.DeleteArrayElementAtIndex(i);
                    break;
                }

                y += EditorGUIUtility.singleLineHeight + 2;
            }

            // Add & Clear buttons
            float buttonHeight = EditorGUIUtility.singleLineHeight + 2;

            Rect addButtonRect = new Rect(position.x, y, position.width * 0.5f - 2, buttonHeight);
            Rect clearButtonRect = new Rect(position.x + position.width * 0.5f + 2, y, position.width * 0.5f - 2, buttonHeight);

            if (GUI.Button(addButtonRect, "+ Add State"))
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
                SerializedProperty newElement = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                SerializedProperty key = newElement.FindPropertyRelative("Key");
                SerializedProperty value = newElement.FindPropertyRelative("Value");

                key.stringValue = GetRandomUnusedKey(seenKeys);
                value.intValue = UnityEngine.Random.Range(1, 5);
            }

            if (GUI.Button(clearButtonRect, "🗑 Clear All"))
            {
                listProp.ClearArray();
            }

            y += buttonHeight;

            // Duplicate key warning
            if (seenKeys.Count < listProp.arraySize)
            {
                Rect warningRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(warningRect, "Duplicate keys detected!", MessageType.Warning);
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty listProp = property.FindPropertyRelative("keyValuePairs");
        if (listProp == null || !listProp.isArray) return EditorGUIUtility.singleLineHeight;

        float height = (listProp.arraySize + 1) * (EditorGUIUtility.singleLineHeight + 2); // rows + buttons
        if (ContainsDuplicateKeys(listProp)) height += EditorGUIUtility.singleLineHeight + 2;
        return height;
    }

    private bool ContainsDuplicateKeys(SerializedProperty listProp)
    {
        HashSet<string> seen = new();
        for (int i = 0; i < listProp.arraySize; i++)
        {
            string key = listProp.GetArrayElementAtIndex(i).FindPropertyRelative("Key").stringValue;
            if (!seen.Add(key)) return true;
        }
        return false;
    }

    private string GetRandomUnusedKey(HashSet<string> usedKeys)
    {
        List<string> unused = new List<string>();
        foreach (string key in sampleKeys)
        {
            if (!usedKeys.Contains(key)) unused.Add(key);
        }

        if (unused.Count == 0)
            return "newState" + UnityEngine.Random.Range(0, 999);

        return unused[UnityEngine.Random.Range(0, unused.Count)];
    }
}
