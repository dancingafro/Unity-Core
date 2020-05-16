using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoreScript.Cursors
{
    public class CursorManagerDataEditor : ExtendedEditorWindow
    {
        public CursorManagerData CursorManagerData { private get; set; }
        string name;
        Rect animationPreviewRect;
        public float timer = 0;
        [MenuItem("Custom Editor/Cursor Manager Editor")]
        static void Init()
        {
            Open(CursorManagerData.Load());
        }
        float editorDeltaTime = 0f;
        float lastTimeSinceStartup = 0f;

        void Update()
        {
            SetEditorDeltaTime();

            if (CursorManagerData.CurrentCursorAnimation != null)
            {
                timer -= editorDeltaTime;
                if (timer <= 0)
                {
                    CursorManagerData.CurrentCursorAnimation.GetNextFrame();
                    timer += CursorManagerData.CurrentCursorAnimation.FrameRate;
                }
            }

        }

        private void SetEditorDeltaTime()
        {
            float timeSinceStartup = (float)EditorApplication.timeSinceStartup;
            if (lastTimeSinceStartup == 0f)
                lastTimeSinceStartup = timeSinceStartup;

            editorDeltaTime = timeSinceStartup - lastTimeSinceStartup;
            lastTimeSinceStartup = timeSinceStartup;
        }

        public static void Open(CursorManagerData cursorManagerData)
        {
            // Get existing open window or if none, make a new one:
            CursorManagerDataEditor window = (CursorManagerDataEditor)EditorWindow.GetWindow(typeof(CursorManagerDataEditor));
            window.CursorManagerData = cursorManagerData;
            window.serializedObject = new SerializedObject(cursorManagerData);
            window.timer = cursorManagerData.CurrentCursorAnimation.FrameRate;
            window.Show();
        }

        bool needsRepaint = false;

        private void OnGUI()
        {
            Draw();

            if (!needsRepaint)
                return;

            needsRepaint = false;
            serializedObject = new SerializedObject(CursorManagerData);
            Repaint();
        }

        void Draw()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("box", GUILayout.MaxWidth(300), GUILayout.ExpandHeight(true));

            DrawField("defaultCursorType", true);
            DrawSidebar(serializedObject.FindProperty("cursorAnimations"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
            if (selectedProperty != null)
                DrawProperties(selectedProperty, true);
            else
                EditorGUILayout.LabelField("Select an item");

            animationPreviewRect = new Rect(position.width - 62, position.height - 62, 50, 50);
            if (CursorManagerData.CurrentCursorAnimation != null && CursorManagerData.CurrentCursorAnimation.CurrentFrame != null)
                GUI.DrawTexture(animationPreviewRect, CursorManagerData.CurrentCursorAnimation.CurrentFrame);


            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            Apply();
        }
        bool hitAdd = false;
        static readonly string ExamplePath = "CoreScript/Cursor";
        protected override void DrawSidebar(SerializedProperty prop)
        {
            int i = 0;
            foreach (SerializedProperty item in prop)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxHeight(10));
                GUILayout.BeginVertical("box", GUILayout.MinWidth(225), GUILayout.ExpandHeight(true));
                if (GUILayout.Button(item.displayName))
                {
                    selectedPropertyPath = item.propertyPath;
                    CursorManagerData.SetActiveCursorAnimation(i);
                    timer = CursorManagerData.CurrentCursorAnimation.FrameRate;
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical("box", GUILayout.MinWidth(25), GUILayout.ExpandHeight(true));
                if (GUILayout.Button("-"))
                {
                    CursorManagerData.RemoveAt(i);
                    needsRepaint = true;
                    return;
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                ++i;
            }

            if (hitAdd)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical("box", GUILayout.MinWidth(225), GUILayout.ExpandHeight(true));
                name = GUILayout.TextArea(name);
                GUILayout.EndVertical();
                GUILayout.BeginVertical("box", GUILayout.MinWidth(25), GUILayout.ExpandHeight(true));
                if (GUILayout.Button("+"))
                {
                    hitAdd = false;
                    string path = ExamplePath + "/Cursor Type/" + name;
                    CursorType cursorType = ScriptableObject.CreateInstance<CursorType>();
                    UnityEditor.AssetDatabase.CreateAsset(cursorType, "Assets/com.desmond.corescript/Core/Resources/" + path + ".asset");
                    UnityEditor.AssetDatabase.SaveAssets();
                    CursorManagerData.AddNewCursorAnimation(new CursorAnimationData(cursorType));
                    needsRepaint = true;
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+"))
            {
                hitAdd = true;
                name = "";
            }

            if (!string.IsNullOrEmpty(selectedPropertyPath))
                selectedProperty = serializedObject.FindProperty(selectedPropertyPath);
        }

        void OnEnable()
        {
            titleContent.text = "Cursor Manager Editor";
            position.Set(position.x, position.y, 400, 300);
            minSize = new Vector2(400, 300);
        }

        void OnDisable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            CursorManagerData.SetDefaultCursorAnimation();
        }
    }
}