using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoreScript.Cursors
{
    public class CursorManagerDataEditor : ExtendedEditorWindow
    {
        public CursorManagerData CursorManagerData { private get; set; }
        public string cursorTypeName = "";
        public double timer = 0;
        CursorType cursorType;
        [MenuItem("Custom Editor/Cursor Manager Editor")]
        static void Init()
        {
            Open(CursorManagerData.Load());
        }
        float editorDeltaTime = 0f;
        float lastTimeSinceStartup = 0f;

        protected override void Update()
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
            CursorManagerDataEditor window = (CursorManagerDataEditor)GetWindow(typeof(CursorManagerDataEditor));
            window.CursorManagerData = cursorManagerData;
            window.serializedObject = new SerializedObject(cursorManagerData);

            if (cursorManagerData.CurrentCursorAnimation != null)
                window.timer = cursorManagerData.CurrentCursorAnimation.FrameRate;
            window.cursorTypeName = "";
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

            if (CursorManagerData.CurrentCursorAnimation != null && CursorManagerData.CurrentCursorAnimation.CurrentFrame != null)
                GUILayout.Box(CursorManagerData.CurrentCursorAnimation.CurrentFrame);

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            Apply();
        }

        bool addNew = false;
        protected override void DrawSidebar(SerializedProperty prop)
        {
            int i = 0;
            foreach (SerializedProperty item in prop)
            {
                GUILayout.BeginHorizontal(GUILayout.MaxHeight(10));
                GUILayout.BeginVertical("box", GUILayout.MinWidth(275), GUILayout.ExpandHeight(true));
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

            GUILayout.BeginHorizontal();

            if (!addNew)
            {
                GUILayout.BeginVertical("box", GUILayout.MinWidth(225), GUILayout.ExpandHeight(true));
                cursorType = (CursorType)EditorGUILayout.ObjectField("Cursor Animation Data :", cursorType, typeof(CursorType), false);
                if (cursorType != null)
                {
                    CursorManagerData.AddNewCursorAnimation(new CursorAnimationData(cursorType));
                    cursorType = null;
                    needsRepaint = true;
                }

                GUILayout.EndVertical();
                GUILayout.BeginVertical("box", GUILayout.MinWidth(50), GUILayout.ExpandHeight(true));
                if (GUILayout.Button("Add new"))
                    addNew = !addNew;
                GUILayout.EndVertical();
            }
            else
            {
                GUILayout.BeginVertical("box", GUILayout.MinWidth(50), GUILayout.ExpandHeight(true));
                GUILayout.Label("Cursor Type :");
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box", GUILayout.MinWidth(225), GUILayout.ExpandHeight(true));
                cursorTypeName = GUILayout.TextArea(cursorTypeName);
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box", GUILayout.MinWidth(25), GUILayout.ExpandHeight(true));
                if (GUILayout.Button("+") && cursorTypeName != "")
                {
                    CursorType cursorType = CreateInstance<CursorType>();

                    if (!AssetDatabase.IsValidFolder("Assets/Resources/CoreScript/Cursor/Cursor Type"))
                    {
                        string[] paths = "Assets/Resources/CoreScript/Cursor/Cursor Type".Split('/');
                        for (int z = 1; z < paths.Length; z++)
                        {
                            if (AssetDatabase.IsValidFolder(paths[z]))
                                continue;

                            AssetDatabase.CreateFolder(paths[z - 1], paths[z]);
                        }
                    }

                    AssetDatabase.CreateAsset(cursorType, "Assets/Resources/CoreScript/Cursor/Cursor Type/" + cursorTypeName + ".asset");
                    AssetDatabase.SaveAssets();
                    CursorManagerData.AddNewCursorAnimation(new CursorAnimationData(cursorType));
                    needsRepaint = true;
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box", GUILayout.MinWidth(50), GUILayout.ExpandHeight(true));
                if (GUILayout.Button("Cancle"))
                    addNew = !addNew;
                GUILayout.EndVertical();
            }

            GUILayout.EndHorizontal();

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