using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreScript.Localisation
{
    [CustomPropertyDrawer(typeof(LocalisedString))]
    public class LocalisedStringDrawer : PropertyDrawer
    {
        bool dropdown = false;
        float height = 0;

        LocalisationData localisationData;

        LocalisationData LocalisationData
        {
            get
            {
                if (!localisationData)
                    localisationData = LocalisationData.Load();

                return localisationData;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return dropdown ? height + 25 : 20;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            position.width -= 34;
            position.height = 18;

            Rect valueRect = new Rect(position)
            {
                x = position.x + 15,
                width = position.width - 15
            };

            Rect foldButtonRect = new Rect(position)
            {
                x = position.x,
                width = 15
            };

            dropdown = EditorGUI.Foldout(foldButtonRect, dropdown, "");

            position.x += 15;
            position.width -= 15;

            SerializedProperty key = property.FindPropertyRelative("key");
            key.stringValue = EditorGUI.TextField(position, key.stringValue);

            position.x += position.width + 2;
            position.width = 17;
            position.height = 17;
            GUIContent searchContent = new GUIContent(Resources.Load<Texture>("CoreScript/Texture/search"));

            if (GUI.Button(position, searchContent))
                TextLocaliserSearchWindow.Open();

            position.x += position.width + 2;

            GUIContent storeContent = new GUIContent(Resources.Load<Texture>("CoreScript/Texture/store"));

            if (GUI.Button(position, storeContent))
                TextLocaliserEditWindow.Open(key.stringValue);

            if (dropdown)
            {
                var value = LocalisationData.GetLocalisedValue(key.stringValue);
                GUIStyle style = GUI.skin.box;
                height = style.CalcHeight(new GUIContent(value), valueRect.width);

                valueRect.height = height;
                valueRect.y += 21;
                EditorGUI.LabelField(valueRect, value, EditorStyles.wordWrappedLabel);
            }

            EditorGUI.EndProperty();
        }
    }
}