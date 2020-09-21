using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreScript.Localisation
{
    public class TextLocaliserEditWindow : EditorWindow
    {

        public static void Open(string key)
        {
            TextLocaliserEditWindow window = new TextLocaliserEditWindow()
            {
                titleContent = new GUIContent("Localiser Window"),
                key = key
            };

            window.ShowUtility();
        }

        public string key, value;

        LocalisationManager localisationManager;

        LocalisationManager LocalisationManager
        {
            get
            {
                if (!localisationManager)
                    localisationManager = LocalisationManager.Instance;

                return localisationManager;
            }
        }

        public void OnGUI()
        {
            key = EditorGUILayout.TextField("Key : ", key);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Value :", GUILayout.MaxWidth(50));

            EditorStyles.textArea.wordWrap = true;
            value = EditorGUILayout.TextArea(value, EditorStyles.textArea, GUILayout.Height(100), GUILayout.Width(400));
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add"))
            {
                if (LocalisationManager.GetLocalisedValue(key) != string.Empty)
                    LocalisationManager.Replace(key, value);
                else
                    LocalisationManager.Add(key, value);
            }
            maxSize = minSize = new Vector2(460, 250);
        }
    }

    public class TextLocaliserSearchWindow : EditorWindow
    {
        public static void Open()
        {
            TextLocaliserSearchWindow window = new TextLocaliserSearchWindow()
            {
                titleContent = new GUIContent("Localiser Window")
            };

            Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Rect rect = new Rect(mouse.x - 450, mouse.y + 10, 10, 10);
            window.ShowAsDropDown(rect, new Vector2(500, 300));

            window.ShowUtility();
        }

        public string value = "";
        public Vector2 scroll;
        LocalisationManager localisationManager;

        LocalisationManager LocalisationManager
        {
            get
            {
                if (!localisationManager)
                    localisationManager = LocalisationManager.Instance;

                return localisationManager;
            }
        }

        public Dictionary<string, string> dictionary;

        private void OnEnable()
        {
            //TODO
            //dictionary = LocalisationManager.GetDictionary();
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginHorizontal("Box");

            EditorGUILayout.LabelField("Search : ", EditorStyles.boldLabel);
            value = EditorGUILayout.TextField(value);

            EditorGUILayout.EndHorizontal();

            GetSearchResult();
        }

        public void GetSearchResult()
        {
            if (value == null)
                return;

            EditorGUILayout.BeginVertical();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            foreach (var item in dictionary)
            {
                if (item.Key.ToLower().Contains(value.ToLower()) || item.Value.ToLower().Contains(value.ToLower()))
                {
                    EditorGUILayout.BeginHorizontal("Box");

                    GUIContent content = new GUIContent(Resources.Load<Texture>("CoreScript/Texture/cross"));

                    if (GUILayout.Button(content, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
                    {
                        if (EditorUtility.DisplayDialog("Remove Key " + item.Key + "?", "This will remove the element from localisation,are you sure?", "do it"))
                        {
                            LocalisationManager.Remove(item.Key);
                            AssetDatabase.Refresh();
                            //TODO
                            //LocalisationManager.RefreshData();
                            //dictionary = LocalisationManager.GetDictionary();
                        }
                    }

                    EditorGUILayout.TextField(item.Key);
                    EditorGUILayout.LabelField(item.Value);

                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

    }
}