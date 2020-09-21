using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CoreScript.Cursors
{
    public class AssetHandler
    {
        [OnOpenAsset()]
        public static bool OpenEditor(int instanceId, int line)
        {
            CursorManagerData obj = EditorUtility.InstanceIDToObject(instanceId) as CursorManagerData;
            if (obj == null)
                return false;

            CursorManagerDataEditor.Open(obj);
            return true;
        }

    }


    [CustomEditor(typeof(CursorManagerData))]
    public class CursorManagerDataDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Open Editor Window"))
                CursorManagerDataEditor.Open((CursorManagerData)target);
        }

    }
}