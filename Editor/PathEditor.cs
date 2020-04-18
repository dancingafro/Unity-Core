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
            }
            bool isClosed = GUILayout.Toggle(Path.IsClosed, "Closed Path");
            if (Path.IsClosed != isClosed)
            {
                Undo.RecordObject(creator, "Closed Path");
                Path.IsClosed = isClosed;
            }
            Path.ControlModeOption controlMode = (Path.ControlModeOption)EditorGUILayout.EnumPopup("Control Mode", Path.ControlMode);
            if (Path.ControlMode != controlMode)
            {
                Undo.RecordObject(creator, "Control Mode");
                Path.ControlMode = controlMode;
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
                    float dstSqr = (Path[i].Position - mousePos).sqrMagnitude;
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
                    OrientedPoint[] points = Path.GetPointsInSegment(i);
                    float dst = HandleUtility.DistancePointBezier(mousePos, points[0].Position, points[3].Position, points[1].Position, points[2].Position);
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
                OrientedPoint[] points = Path.GetPointsInSegment(i);
                if (creator.displayControlPoints && Path.ControlMode != Path.ControlModeOption.Automatic)
                {
                    Handles.color = Color.black;
                    Handles.DrawLine(points[1].Position, points[0].Position);
                    Handles.DrawLine(points[2].Position, points[3].Position);
                }
                Color segmentColour = selectedSegmentIndex == i ? creator.selectedCol : creator.segmentCol;
                Handles.DrawBezier(points[0].Position, points[3].Position, points[1].Position, points[2].Position, segmentColour, null, 2);
            }

            for (int i = 0; i < Path.NumPoints; ++i)
            {
                bool isAnchor = i % 3 == 0;

                if (!isAnchor && !creator.displayControlPoints)
                    continue;

                Handles.color = isAnchor ? creator.anchorCol : creator.controlCol;
                Vector3 newPos = Handles.FreeMoveHandle(Path[i].Position, Path[i].Rotation, isAnchor ? creator.anchorDiameter : creator.controlDiameter, Vector3.zero, Handles.CylinderHandleCap);

                if (Path[i].Position == newPos)
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