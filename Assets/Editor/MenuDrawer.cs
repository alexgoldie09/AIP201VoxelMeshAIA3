using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Custom drawer for the MenuManager's menu dictionary.
/// </summary>
[CustomPropertyDrawer(typeof(SerializableDictionary<int, string>))]
public class MenuDrawer : PropertyDrawer
{
    private const float keyWidth = 50f;
    private const float buttonWidth = 20f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty listProp = property.FindPropertyRelative("keyValuePairs");

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel(position, label);

        if (listProp != null && listProp.isArray)
        {
            float y = position.y;
            HashSet<int> seenKeys = new();

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty pair = listProp.GetArrayElementAtIndex(i);
                SerializedProperty key = pair.FindPropertyRelative("Key");
                SerializedProperty value = pair.FindPropertyRelative("Value");

                Rect keyRect = new Rect(position.x, y, keyWidth, EditorGUIUtility.singleLineHeight);
                Rect valueRect = new Rect(position.x + keyWidth + 5, y, position.width - keyWidth - buttonWidth - 10, EditorGUIUtility.singleLineHeight);
                Rect removeButtonRect = new Rect(position.x + position.width - buttonWidth, y, buttonWidth, EditorGUIUtility.singleLineHeight);

                bool isDuplicate = !seenKeys.Add(key.intValue);
                if (isDuplicate)
                    EditorGUI.DrawRect(new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight), new Color(1f, 0.5f, 0.5f, 0.25f));

                // Draw the key and value
                EditorGUI.PropertyField(keyRect, key, GUIContent.none);
                EditorGUI.PropertyField(valueRect, value, GUIContent.none);

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

            if (GUI.Button(addButtonRect, "+ Add Menu Item"))
            {
                listProp.InsertArrayElementAtIndex(listProp.arraySize);
                SerializedProperty newElement = listProp.GetArrayElementAtIndex(listProp.arraySize - 1);
                newElement.FindPropertyRelative("Key").intValue = GetUniqueID(seenKeys);
                newElement.FindPropertyRelative("Value").stringValue = "New Dish";
            }

            if (GUI.Button(clearButtonRect, "Clear Menu"))
            {
                listProp.ClearArray();
            }

            y += buttonHeight;

            if (seenKeys.Count < listProp.arraySize)
            {
                Rect warningRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.HelpBox(warningRect, "Duplicate menu IDs detected!", MessageType.Warning);
            }
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty listProp = property.FindPropertyRelative("keyValuePairs");
        if (listProp == null || !listProp.isArray) return EditorGUIUtility.singleLineHeight;

        float height = (listProp.arraySize + 1) * (EditorGUIUtility.singleLineHeight + 2);
        if (ContainsDuplicateKeys(listProp)) height += EditorGUIUtility.singleLineHeight + 2;
        return height;
    }

    private bool ContainsDuplicateKeys(SerializedProperty listProp)
    {
        HashSet<int> seen = new();
        for (int i = 0; i < listProp.arraySize; i++)
        {
            int key = listProp.GetArrayElementAtIndex(i).FindPropertyRelative("Key").intValue;
            if (!seen.Add(key)) return true;
        }
        return false;
    }

    private int GetUniqueID(HashSet<int> usedKeys)
    {
        int id = Random.Range(1, 9999);
        while (usedKeys.Contains(id))
        {
            id = Random.Range(1, 9999);
        }
        return id;
    }
}
