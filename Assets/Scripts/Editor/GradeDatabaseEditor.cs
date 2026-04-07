using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GradeDatabase))]
public class GradeDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawGrade("Grade A", "gradeA");
        DrawGrade("Grade B", "gradeB");
        DrawGrade("Grade C", "gradeC");

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGrade(string label, string propertyName)
    {
        SerializedProperty gradeProp = serializedObject.FindProperty(propertyName);
        if (gradeProp == null) return;

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

        SerializedProperty spriteProp = gradeProp.FindPropertyRelative("gradeSprite");
        EditorGUILayout.PropertyField(spriteProp);

        SerializedProperty itemsProp = gradeProp.FindPropertyRelative("items");
        itemsProp.isExpanded = EditorGUILayout.Foldout(itemsProp.isExpanded, $"Items ({itemsProp.arraySize})");

        if (itemsProp.isExpanded)
        {
            EditorGUI.indentLevel++;
            itemsProp.arraySize = EditorGUILayout.IntField("Size", itemsProp.arraySize);

            for (int i = 0; i < itemsProp.arraySize; i++)
            {
                SerializedProperty item = itemsProp.GetArrayElementAtIndex(i);
                SerializedProperty numProp = item.FindPropertyRelative("number");
                SerializedProperty nameProp = item.FindPropertyRelative("itemName");
                string elementName = string.IsNullOrEmpty(nameProp.stringValue)
                    ? numProp.intValue.ToString()
                    : $"{numProp.intValue} - {nameProp.stringValue}";

                item.isExpanded = EditorGUILayout.Foldout(item.isExpanded, elementName);
                if (item.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(numProp);
                    EditorGUILayout.PropertyField(nameProp);
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("itemSprite"));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("description"));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("price"));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("scriptPrefab"));
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }
}
