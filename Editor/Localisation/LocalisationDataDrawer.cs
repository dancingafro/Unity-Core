using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CoreScript.Localisation
{
    public class AssetHandler
    {
        [OnOpenAsset()]
        public static bool OpenEditor(int instanceId, int line)
        {
            LocalisationData obj = EditorUtility.InstanceIDToObject(instanceId) as LocalisationData;
            if (obj == null)
                return false;

            LocalisationDataEditor.Open(obj);
            return true;
        }

    }


    [CustomEditor(typeof(LocalisationData))]
    public class LocalisationDataDrawer : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "languages", "CSVFiles", "csvDatas");
            if (GUILayout.Button("Open Editor Window"))
                LocalisationDataEditor.Open((LocalisationData)target);
        }
    }
}