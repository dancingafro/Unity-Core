using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CoreScript.Utility
{
    [CustomEditor(typeof(PathCreator))]
    public class PathEditor : Editor
    {
        PathCreator creator;
        Path path;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (GUILayout.Button("Create new"))
            {
                Undo.RecordObject(creator, "Create new");
                creator.CreatePath();
                path = creator.path;
                SceneView.RepaintAll();
            }

            if (GUILayout.Button("Toggle Closed"))
            {
                Undo.RecordObject(creator, "Toggle Closed");
                path.ToggleClosed();
            }

            bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Controls Points");
            if (path.AutoSetControlPoints != autoSetControlPoints)
            {
                Undo.RecordObject(creator, "Auto Set Control Points");
                path.AutoSetControlPoints = autoSetControlPoints;
            }

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        private void OnSceneGUI()
        {
            Input();
            Draw();
        }

        void Input()
        {
            Event guiEvent = Event.current;
            Vector2 mosPos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                Undo.RecordObject(creator, "Add Segment");
                path.AddSegment(mosPos);
            }
        }

        void Draw()
        {
            for (int i = 0; i < path.NumSegments; ++i)
            {
                Vector2[] points = path.GetPointsInSegment(i);
                Handles.color = Color.black;
                Handles.DrawLine(points[1], points[0]);
                Handles.DrawLine(points[2], points[3]);
                Handles.DrawBezier(points[0], points[3], points[1], points[2], Color.green, null, 2);
            }

            for (int i = 0; i < path.NumPoints; ++i)
            {
                Vector2 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, .1f, Vector2.zero, Handles.CylinderHandleCap);

                if (path[i] == newPos)
                    continue;

                Undo.RecordObject(creator, "Move point");
                path.MovePoint(i, newPos);
            }
        }

        private void OnEnable()
        {
            creator = (PathCreator)target;
            if (creator.path == null)
                creator.CreatePath();

            path = creator.path;
        }
    }
}