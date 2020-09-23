using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;

namespace CoreScript.Localisation
{
    public class LocalisationDataEditor : ExtendedEditorWindow
    {
        public LocalisationData LocalisationData { private get; set; }
        [MenuItem("Custom Editor/Localisation Editor")]
        static void Init()
        {
            Open(LocalisationData.Load());
        }

        public static void Open(LocalisationData localisationData)
        {
            // Get existing open window or if none, make a new one:
            LocalisationDataEditor window = (LocalisationDataEditor)GetWindow(typeof(LocalisationDataEditor));
            window.LocalisationData = localisationData;
            window.serializedObject = new SerializedObject(localisationData);
            window.Show();
        }

        bool needsRepaint = false, addNew = false;
        Languages selectedLanguge;
        Dictionary<string, string> selectedDictionary;
        List<string> keys = new List<string>(), values = new List<string>();
        private void OnGUI()
        {
            DrawHeader();

            PrimaryBody();

            Apply();
            if (!needsRepaint)
                return;

            needsRepaint = false;
            serializedObject = new SerializedObject(LocalisationData);
            Repaint();
        }

        void DrawHeader()
        {
            float width = (position.width - 150) * .5f;
            EditorGUILayout.BeginHorizontal("box", GUILayout.MaxHeight(20), GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical("box", GUILayout.Width(width));
            DrawField("csvPath", true);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box", GUILayout.Width(width));
            DrawField("languagePath", true);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginHorizontal("box", GUILayout.Width(150));

            EditorGUILayout.BeginVertical("box", GUILayout.Width(60));
            if (GUILayout.Button("Reset"))
                LocalisationData.LoadLocalisation(true);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.Width(60));
            if (GUILayout.Button("Refresh"))
                LocalisationData.LoadLocalisation();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }

        void PrimaryBody()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(150), GUILayout.ExpandHeight(true));

            DrawSideTab(LocalisationData.Languages);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            if (selectedLanguge != null)
                DrawDetails();
            else
                EditorGUILayout.LabelField("Select an item");

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        protected void DrawSideTab(Languages[] languages)
        {
            foreach (Languages item in languages)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxHeight(10));
                GUILayout.BeginVertical("box", GUILayout.Width(150), GUILayout.ExpandHeight(true));
                if (GUILayout.Button(item.name))
                    RefreshSelectedDictionary(item);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

        void RefreshSelectedDictionary(Languages item)
        {
            selectedLanguge = item;
            selectedDictionary = LocalisationData.GetDictionary(item.Header);
            keys.Clear();
            values.Clear();
            foreach (var key in selectedDictionary.Keys)
                keys.Add(key);
            foreach (var value in selectedDictionary.Values)
                values.Add(value);
        }
        string tempKey, tempValue;
        protected void DrawDetails()
        {
            GUILayout.BeginVertical();
            GUILayout.Label(selectedLanguge.name);
            GUILayout.EndVertical();

            GUILayout.BeginHorizontal(GUILayout.MaxHeight(40));

            GUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            EditorGUI.BeginChangeCheck();
            GUILayout.Label("Header : ");
            selectedLanguge.Header = GUILayout.TextField(selectedLanguge.Header);
            GUILayout.Label("Default Language : ");
            selectedLanguge.DefaultLanguage = GUILayout.Toggle(selectedLanguge.DefaultLanguage, "");
            if (EditorGUI.EndChangeCheck())
                needsRepaint = true;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            float netWidth = position.width - 160;
            float width = netWidth * .4f;
            netWidth = (netWidth - width * 2) * .45f;
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(width));
            GUILayout.Label("Keys");
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.BeginVertical(GUILayout.Width(width));
            GUILayout.Label("Value");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            for (int i = 0; i < keys.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(width));
                keys[i] = GUILayout.TextField(keys[i]);
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginVertical(GUILayout.Width(width));
                values[i] = GUILayout.TextField(values[i]);
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginVertical(GUILayout.Width(netWidth));
                if (GUILayout.Button("Update"))
                    Edit(selectedDictionary.ElementAt(i).Key, keys[i], values[i]);
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginVertical(GUILayout.Width(netWidth));
                if (GUILayout.Button("Remove"))
                {
                    LocalisationData.Remove(selectedDictionary.ElementAt(i).Key);
                    LocalisationData.LoadLocalisation();
                    RefreshSelectedDictionary(selectedLanguge);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUILayout.BeginHorizontal();

            if (!addNew)
            {
                if (GUILayout.Button("Add"))
                {
                    tempKey = "";
                    tempValue = "";
                    addNew = !addNew;
                }
            }
            else
            {
                GUILayout.BeginVertical(GUILayout.Width(width));
                tempKey = GUILayout.TextField(tempKey);
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginVertical(GUILayout.Width(width));
                tempValue = GUILayout.TextField(tempValue);
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginVertical(GUILayout.Width(netWidth));
                if (GUILayout.Button("Add"))
                {
                    LocalisationData.Add(tempKey, tempValue);
                    LocalisationData.LoadLocalisation();
                    RefreshSelectedDictionary(selectedLanguge);
                    tempKey = "";
                    tempValue = "";
                    addNew = !addNew;
                }
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginVertical(GUILayout.Width(netWidth));
                if (GUILayout.Button("Cancel"))
                {
                    tempKey = "";
                    tempValue = "";
                    addNew = !addNew;
                }
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();
        }

        void Edit(string prevKey, string key, string value)
        {
            if (prevKey != key)
            {
                key = key.ToLower();
                key = key.Replace(" ", "_");
                Dictionary<string, Dictionary<string, string>> allTemp = LocalisationData.GetAllDictionary();

                LocalisationData.Remove(prevKey, false);
                LocalisationData.Add(key, "", "", false);
                foreach (var item in allTemp.Keys)
                {
                    if (selectedLanguge.Header == item)
                        continue;

                    LocalisationData.Edit(key, allTemp[item][prevKey], item, false);
                }
            }

            LocalisationData.Edit(key, value, selectedLanguge.Header);
            LocalisationData.LoadLocalisation();
            RefreshSelectedDictionary(selectedLanguge);
        }

        void OnEnable()
        {
            titleContent.text = "Localisation Data Editor";
            position.Set(position.x, position.y, 700, 400);
            minSize = new Vector2(700, 400);
        }

        void OnDisable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}