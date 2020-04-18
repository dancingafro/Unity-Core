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
        Path Path { get { return creator.path; } }

        const float segmentSelectDstThreshold = .1f;
        int selectedSegmentIndex = -1;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            if (GUILayout.Button("Create new"))
            {
                Undo.RecordObject(creator, "Create new");
                creator.CreatePath();
                SceneView.RepaintAll();
            }
            bool isClosed = GUILayout.Toggle(Path.IsClosed, "Closed Path");
            if (Path.IsClosed != isClosed)
            {
                Undo.RecordObject(creator, "Closed Path");
                Path.IsClosed = isClosed;
            }

            bool autoSetControlPoints = GUILayout.Toggle(Path.AutoSetControlPoints, "Auto Set Controls Points");
            if (Path.AutoSetControlPoints != autoSetControlPoints)
            {
                Undo.RecordObject(creator, "Auto Set Control Points");
                Path.AutoSetControlPoints = autoSetControlPoints;
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
            Vector3 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
            {
                if (selectedSegmentIndex != -1)
                {
                    Undo.RecordObject(creator, "Split Segment");
                    Path.SplitSegment(mousePos, selectedSegmentIndex);
                }
                else if (!Path.IsClosed)
                {
                    Undo.RecordObject(creator, "Add Segment");
                    Path.AddSegment(mousePos);
                }
            }

            if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
            {
                float minDstToAnchorSqr = .05f * .05f;
                int closestAnchorIndex = -1;

                for (int i = 0; i < Path.NumPoints; i += 3)
                {
                    float dstSqr = (Path[i] - mousePos).sqrMagnitude;
                    if (dstSqr < minDstToAnchorSqr)
                    {
                        minDstToAnchorSqr = dstSqr;
                        closestAnchorIndex = i;
                    }
                }

                if (closestAnchorIndex != -1)
                {
                    Undo.RecordObject(creator, "Remove Segment");
                    Path.DeleteSegment(closestAnchorIndex);
                }
            }
            if (guiEvent.type == EventType.MouseMove)
            {
                float minDstToSegment = segmentSelectDstThreshold;
                int newSelectedSegmentIndex = -1;

                for (int i = 0; i < Path.NumSegments; ++i)
                {
                    Vector3[] points = Path.GetPointsInSegment(i);
                    float dst = HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                    if (dst < minDstToSegment)
                    {
                        minDstToSegment = dst;
                        newSelectedSegmentIndex = i;
                    }
                }
                if (selectedSegmentIndex != newSelectedSegmentIndex)
                {
                    selectedSegmentIndex = newSelectedSegmentIndex;
                    SceneView.RepaintAll();
                }
            }
            HandleUtility.AddDefaultControl(0);
        }

        void Draw()
        {
            for (int i = 0; i < Path.NumSegments; ++i)
            {
                Vector3[] points = Path.GetPointsInSegment(i);
                if (creator.displayControlPoints)
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(points[1], points[0]);
                    Handles.DrawLine(points[2], points[3]);
                }
                Color segmentColour = selectedSegmentIndex == i ? creator.selectedCol : creator.segmentCol;
                Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentColour, null, 2);
            }

            for (int i = 0; i < Path.NumPoints; ++i)
            {
                bool isAnchor = i % 3 == 0;

                if (!isAnchor && !creator.displayControlPoints)
                    continue;

                Handles.color = isAnchor ? creator.anchorCol : creator.controlCol;
                Vector3 newPos = Handles.FreeMoveHandle(Path[i], Quaternion.identity, isAnchor ? creator.anchorDiameter : creator.controlDiameter, Vector3.zero, Handles.CylinderHandleCap);

                if (Path[i] == newPos)
                    continue;

                Undo.RecordObject(creator, "Move point");
                Path.MovePoint(i, newPos);
            }
        }

        private void OnEnable()
        {
            creator = (PathCreator)target;
            if (creator.path == null)
                creator.CreatePath();
        }
    }
}