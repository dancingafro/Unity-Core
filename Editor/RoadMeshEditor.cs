using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoreScript.Utility
{
    [CustomEditor(typeof(RoadMeshCreator))]
    public class RoadMeshEditor : Editor
    {
        RoadMeshCreator creator;

        void OnEnable()
        {
            creator = (RoadMeshCreator)target;
        }

        void OnSceneGUI()
        {
            if (creator.autoUpdate && Event.current.type == EventType.Repaint)
                creator.UpdateRoad();
        }
    }
}