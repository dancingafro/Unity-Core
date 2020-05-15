using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoreScript.Cursors
{
    public class CursorManagerDataEditor : EditorWindow
    {
        public CursorManagerData CursorManagerData { private get; set; }
        const int borderSize = 10;
        const float keyWidth = 10;
        const float keyHeight = 20;

        [MenuItem("Custom Editor/Cursor Manager Editor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            CursorManagerDataEditor window = (CursorManagerDataEditor)EditorWindow.GetWindow(typeof(CursorManagerDataEditor));
            window.CursorManagerData = CursorManagerData.Load();
            window.Show();
        }

        bool needsRepaint = false;

        private void OnGUI()
        {

            Draw();

            if (!needsRepaint)
                return;

            needsRepaint = false;
            Repaint();
        }

        void Draw()
        {
        }

        private void OnEnable()
        {
            titleContent.text = "Cursor Manager Editor";
            position.Set(position.x, position.y, 400, 300);
            minSize = new Vector2(400, 300);
            maxSize = new Vector2(1920, 1080);
        }

        private void OnDisable()
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
    }
}