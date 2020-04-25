using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.PathCreation
{
    public class PathCreator : MonoBehaviour
    {
        public event System.Action PathUpdated;

        [SerializeField, HideInInspector]
        PathCreatorData editorData;
        [SerializeField, HideInInspector]
        bool initialized;


        // Vertex path created from the current bezier path
        public VertexPath VertexPath
        {
            get
            {
                if (!initialized)
                    InitializeEditorData(false);

                return editorData.GetVertexPath(transform);
            }
        }

        // The bezier path created in the editor
        public BezierPath BezierPath
        {
            get
            {
                if (!initialized)
                    InitializeEditorData(false);

                return editorData.BezierPath;
            }
            set
            {
                if (!initialized)
                    InitializeEditorData(false);

                editorData.BezierPath = value;
            }
        }

        #region Internal methods

        /// Used by the path editor to initialise some data
        public void InitializeEditorData(bool in2DMode)
        {
            if (editorData == null)
                editorData = new PathCreatorData();

            editorData.bezierOrVertexPathModified -= TriggerPathUpdate;
            editorData.bezierOrVertexPathModified += TriggerPathUpdate;

            editorData.Initialize(in2DMode);
            initialized = true;
        }

        public PathCreatorData EditorData { get { return editorData; } }

        public void TriggerPathUpdate() { PathUpdated?.Invoke(); }

#if UNITY_EDITOR

        GlobalDisplaySettings globalEditorDisplaySettings;
        GlobalDisplaySettings GlobalEditorDisplaySettings
        {
            get
            {
                if (globalEditorDisplaySettings == null)
                    globalEditorDisplaySettings = GlobalDisplaySettings.Load();

                return globalEditorDisplaySettings;
            }
        }

        // Draw the path when path objected is not selected (if enabled in settings)
        void OnDrawGizmos()
        {
            // Only draw path gizmo if the path object is not selected
            // (editor script is resposible for drawing when selected)
            GameObject selectedObj = UnityEditor.Selection.activeGameObject;
            if (selectedObj == gameObject || VertexPath == null)
                return;

            VertexPath.UpdateTransform(transform);

            if (!GlobalEditorDisplaySettings.visibleWhenNotSelected)
                return;

            Gizmos.color = GlobalEditorDisplaySettings.BezierPath;

            for (int i = 0; i < VertexPath.NumPoints; i++)
            {
                int nextI = i + 1;
                if (nextI >= VertexPath.NumPoints)
                {
                    if (!VertexPath.isClosedLoop)
                        break;

                    nextI %= VertexPath.NumPoints;
                }
                Gizmos.DrawLine(VertexPath.GetPoint(i), VertexPath.GetPoint(nextI));
            }
        }
#endif

        #endregion
    }
}