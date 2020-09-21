using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreScript.Utility
{
    [CustomPropertyDrawer(typeof(CustomColourGradient))]
    public class GradientColourDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Event guiEvent = Event.current;
            CustomColourGradient gradient = (CustomColourGradient)fieldInfo.GetValue(property.serializedObject.targetObject);

            float labelWidth = GUI.skin.label.CalcSize(label).x + 5;
            Rect textureRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && textureRect.Contains(guiEvent.mousePosition))
                EditorWindow.GetWindow<CustomColourGradientEditor>().Gradient = gradient;
            else
            {
                GUI.Label(position, label);
                GUIStyle gradientStyle = new GUIStyle();
                gradientStyle.normal.background = gradient.GetTexture((int)position.width);
                GUI.Label(textureRect, GUIContent.none, gradientStyle);
            }
        }
    }

    [CustomPropertyDrawer(typeof(CustomTextureGradient))]
    public class GradientTextureDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Event guiEvent = Event.current;
            CustomTextureGradient gradient = (CustomTextureGradient)fieldInfo.GetValue(property.serializedObject.targetObject);

            float labelWidth = GUI.skin.label.CalcSize(label).x + 5;
            Rect textureRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && textureRect.Contains(guiEvent.mousePosition))
                EditorWindow.GetWindow<CustomTextureGradientEditor>().Gradient = gradient;
            else
            {
                GUI.Label(position, label);
                GUIStyle gradientStyle = new GUIStyle();
                gradientStyle.normal.background = gradient.GetTexture((int)position.width);
                GUI.Label(textureRect, GUIContent.none, gradientStyle);
            }
        }
    }
}