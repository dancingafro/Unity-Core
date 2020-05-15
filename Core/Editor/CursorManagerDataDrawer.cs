using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CoreScript.Cursors
{
    [CustomEditor(typeof(CursorManagerData))]
    public class CursorManagerDataDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("Open Editor Window"))
                EditorWindow.GetWindow<CursorManagerDataEditor>().CursorManagerData = (CursorManagerData)target;
        }

    }
}