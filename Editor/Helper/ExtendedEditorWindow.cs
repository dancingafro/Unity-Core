using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ExtendedEditorWindow : EditorWindow
{
    protected SerializedObject serializedObject;
    protected SerializedProperty currentProperty;
    protected SerializedProperty selectedProperty;
    protected string selectedPropertyPath;

    protected virtual void DrawProperties(SerializedProperty prop, bool drawChildren)
    {
        string lastPropPath = string.Empty;
        foreach (SerializedProperty p in prop)
        {
            if (!string.IsNullOrEmpty(lastPropPath) && p.propertyPath.Contains(lastPropPath))
                continue;
            lastPropPath = p.propertyPath;
            EditorGUILayout.PropertyField(p, drawChildren);
        }
    }

    protected virtual void Update()
    {
        
    }

    protected virtual void DrawSidebar(SerializedProperty prop)
    {
        foreach (SerializedProperty item in prop)
        {
            if (GUILayout.Button(item.displayName))
                selectedPropertyPath = item.propertyPath;
        }
        if (!string.IsNullOrEmpty(selectedPropertyPath))
            selectedProperty = serializedObject.FindProperty(selectedPropertyPath);
    }

    protected void DrawField(string propName, bool relative)
    {
        if (relative && currentProperty != null)
            EditorGUILayout.PropertyField(currentProperty.FindPropertyRelative(propName), true);
        else if (serializedObject != null)
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propName), true);

    }
    protected void Apply()
    {
        serializedObject.ApplyModifiedProperties();
    }
}
