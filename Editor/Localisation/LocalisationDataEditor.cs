using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

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
                {
                    selectedLanguge = item;
                    selectedDictionary = LocalisationData.GetDictionary(item.Header);
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
        }

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


            float width = (position.width - 160) * .5f;
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
            foreach (var item in selectedDictionary.Keys)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(width));
                GUILayout.Label(item);
                GUILayout.EndVertical();
                GUILayout.Space(10);
                GUILayout.BeginVertical(GUILayout.Width(width));
                GUILayout.Label(selectedDictionary[item]);
                //selectedDictionary[item] = GUILayout.TextField(selectedDictionary[item]);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }


            //GUILayout.EndVertical();
            GUILayout.EndVertical();
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