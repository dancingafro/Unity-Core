using System.Collections.Generic;
using CoreScript.Utility;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CoreScript.PathCreation
{
    /// Editor class for the creation of Bezier and Vertex paths

    [CustomEditor(typeof(PathCreator))]
    public class PathEditor : Editor
    {

        #region Fields

        // Interaction:
        const float segmentSelectDistanceThreshold = 10f;
        const float screenPolylineMaxAngleError = .3f;
        const float screenPolylineMinVertexDst = .01f;

        // Help messages:
        const string helpInfo = "Shift-click to add or insert new points. Control-click to delete points. For more detailed infomation, please refer to the documentation.";
        static readonly string[] spaceNames = { "3D (xyz)", "2D (xy)", "Top-down (xz)" };
        static readonly string[] tabNames = { "Bézier Path", "Vertex Path" };
        const string constantSizeTooltip = "If true, anchor and control points will keep a constant size when zooming in the editor.";

        // Display
        const int inspectorSectionSpacing = 10;
        const float constantHandleScale = .01f;
        const float normalsSpacing = .2f;
        GUIStyle boldFoldoutStyle;

        // References:
        PathCreator creator;
        Editor globalDisplaySettingsEditor;
        ScreenSpacePolyLine screenSpaceLine;
        ScreenSpacePolyLine.MouseInfo pathMouseInfo;
        GlobalDisplaySettings globalDisplaySettings;
        PathHandle.HandleColours splineAnchorColours;
        PathHandle.HandleColours splineControlColours;
        Dictionary<GlobalDisplaySettings.HandleType, Handles.CapFunction> capFunctions;
        ArcHandle anchorAngleHandle = new ArcHandle();
        VertexPath normalsVertexPath;

        // State variables:
        int selectedSegmentIndex;
        int draggingHandleIndex;
        int mouseOverHandleIndex;
        int handleIndexToDisplayAsTransform;

        bool shiftLastFrame;
        bool hasUpdatedScreenSpaceLine;
        bool hasUpdatedNormalsVertexPath;
        bool editingNormalsOld;

        Vector3 transformPos;
        Vector3 transformScale;
        Quaternion transformRot;

        Color handlesStartCol;

        // Constants
        const int bezierPathTab = 0;
        const int vertexPathTab = 1;

        public GlobalDisplaySettings GlobalDisplaySettings
        {
            get
            {
                if (globalDisplaySettings == null)
                    globalDisplaySettings = GlobalDisplaySettings.Load();

                return globalDisplaySettings;
            }
        }

        #endregion

        #region Inspectors

        public override void OnInspectorGUI()
        {
            // Initialize GUI styles
            if (boldFoldoutStyle == null)
            {
                boldFoldoutStyle = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold
                };
            }

            Undo.RecordObject(creator, "Path settings changed");

            // Draw Bezier and Vertex tabs
            int tabIndex = GUILayout.Toolbar(Data.tabIndex, tabNames);
            if (tabIndex != Data.tabIndex)
            {
                Data.tabIndex = tabIndex;
                TabChanged();
            }

            // Draw inspector for active tab
            switch (Data.tabIndex)
            {
                case bezierPathTab:
                    DrawBezierPathInspector();
                    break;
                case vertexPathTab:
                    DrawVertexPathInspector();
                    break;
            }

            // Notify of undo/redo that might modify the VertexPath
            if (Event.current.type == EventType.ValidateCommand && Event.current.commandName == "UndoRedoPerformed")
                Data.PathModifiedByUndo();
        }

        void DrawBezierPathInspector()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                // Path options:
                Data.showPathOptions = EditorGUILayout.Foldout(Data.showPathOptions, new GUIContent("Bézier Path Options"), true, boldFoldoutStyle);
                if (Data.showPathOptions)
                {
                    BezierPath.Space = (PathSpace)EditorGUILayout.Popup("Space", (int)BezierPath.Space, spaceNames);
                    BezierPath.ControlPointMode = (BezierPath.ControlMode)EditorGUILayout.EnumPopup(new GUIContent("Control Mode"), BezierPath.ControlPointMode);
                    if (BezierPath.ControlPointMode == BezierPath.ControlMode.Automatic)
                        BezierPath.AutoControlLength = EditorGUILayout.Slider(new GUIContent("Control Spacing"), BezierPath.AutoControlLength, 0, 1);

                    BezierPath.IsClosed = EditorGUILayout.Toggle("Closed Path", BezierPath.IsClosed);
                    Data.showTransformTool = EditorGUILayout.Toggle(new GUIContent("Enable Transforms"), Data.showTransformTool);

                    Tools.hidden = !Data.showTransformTool;

                    // Check if out of bounds (can occur after undo operations)
                    if (handleIndexToDisplayAsTransform >= BezierPath.NumPoints)
                        handleIndexToDisplayAsTransform = -1;

                    // If a point has been selected
                    if (handleIndexToDisplayAsTransform != -1)
                    {
                        EditorGUILayout.LabelField("Selected Point:");

                        using (new EditorGUI.IndentLevelScope())
                        {
                            var currentPosition = creator.BezierPath[handleIndexToDisplayAsTransform];
                            var newPosition = EditorGUILayout.Vector3Field("Position", currentPosition);
                            if (newPosition != currentPosition)
                            {
                                Undo.RecordObject(creator, "Move point");
                                creator.BezierPath.MovePoint(handleIndexToDisplayAsTransform, newPosition);
                            }
                            // Don't draw the angle field if we aren't selecting an anchor point/not in 3d space
                            if (handleIndexToDisplayAsTransform % 3 == 0 && creator.BezierPath.Space == PathSpace.xyz)
                            {
                                var anchorIndex = handleIndexToDisplayAsTransform / 3;
                                var currentAngle = creator.BezierPath.GetAnchorNormalAngle(anchorIndex);
                                var newAngle = EditorGUILayout.FloatField("Angle", currentAngle);
                                if (newAngle != currentAngle)
                                {
                                    Undo.RecordObject(creator, "Set Angle");
                                    creator.BezierPath.SetAnchorNormalAngle(anchorIndex, newAngle);
                                }
                            }
                        }
                    }

                    if (Data.showTransformTool & (handleIndexToDisplayAsTransform == -1))
                    {
                        if (GUILayout.Button("Centre Transform"))
                        {
                            Vector3 worldCentre = BezierPath.CalculateBoundsWithTransform(creator.transform).center;
                            Vector3 transformPos = creator.transform.position;

                            if (BezierPath.Space == PathSpace.xy)
                                transformPos = new Vector3(transformPos.x, transformPos.y, 0);
                            else if (BezierPath.Space == PathSpace.xz)
                                transformPos = new Vector3(transformPos.x, 0, transformPos.z);

                            Vector3 worldCentreToTransform = transformPos - worldCentre;

                            if (worldCentre != creator.transform.position)
                            {
                                //Undo.RecordObject (creator, "Centralize Transform");
                                if (worldCentreToTransform != Vector3.zero)
                                {
                                    Vector3 localCentreToTransform = MathUtility.InverseTransformVector(worldCentreToTransform, creator.transform, BezierPath.Space);
                                    for (int i = 0; i < BezierPath.NumPoints; i++)
                                        BezierPath.SetPoint(i, BezierPath.GetPoint(i) + localCentreToTransform, true);
                                }

                                creator.transform.position = worldCentre;
                                BezierPath.NotifyPathModified();
                            }
                        }
                    }

                    if (GUILayout.Button("Reset Path"))
                    {
                        Undo.RecordObject(creator, "Reset Path");
                        bool in2DEditorMode = EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D;
                        Data.ResetBezierPath(creator.transform.position, in2DEditorMode);
                        EditorApplication.QueuePlayerLoopUpdate();
                    }

                    GUILayout.Space(inspectorSectionSpacing);
                }

                Data.showNormals = EditorGUILayout.Foldout(Data.showNormals, new GUIContent("Normals Options"), true, boldFoldoutStyle);
                if (Data.showNormals)
                {
                    BezierPath.FlipNormals = EditorGUILayout.Toggle(new GUIContent("Flip Normals"), BezierPath.FlipNormals);
                    if (BezierPath.Space == PathSpace.xyz)
                    {
                        BezierPath.GlobalNormalsAngle = EditorGUILayout.Slider(new GUIContent("Global Angle"), BezierPath.GlobalNormalsAngle, 0, 360);

                        if (GUILayout.Button("Reset Normals"))
                        {
                            Undo.RecordObject(creator, "Reset Normals");
                            BezierPath.FlipNormals = false;
                            BezierPath.ResetNormalAngles();
                        }
                    }
                    GUILayout.Space(inspectorSectionSpacing);
                }

                // Editor display options
                Data.showDisplayOptions = EditorGUILayout.Foldout(Data.showDisplayOptions, new GUIContent("Display Options"), true, boldFoldoutStyle);
                if (Data.showDisplayOptions)
                {
                    Data.showPathBounds = GUILayout.Toggle(Data.showPathBounds, new GUIContent("Show Path Bounds"));
                    Data.showPerSegmentBounds = GUILayout.Toggle(Data.showPerSegmentBounds, new GUIContent("Show Segment Bounds"));
                    Data.displayAnchorPoints = GUILayout.Toggle(Data.displayAnchorPoints, new GUIContent("Show Anchor Points"));

                    if (!(BezierPath.ControlPointMode == BezierPath.ControlMode.Automatic && GlobalDisplaySettings.hideAutoControls))
                        Data.displayControlPoints = GUILayout.Toggle(Data.displayControlPoints, new GUIContent("Show Control Points"));

                    Data.keepConstantHandleSize = GUILayout.Toggle(Data.keepConstantHandleSize, new GUIContent("Constant Point Size", constantSizeTooltip));
                    Data.bezierHandleScale = Mathf.Max(0, EditorGUILayout.FloatField(new GUIContent("Handle Scale"), Data.bezierHandleScale));
                    DrawGlobalDisplaySettingsInspector();
                }

                if (check.changed)
                {
                    SceneView.RepaintAll();
                    EditorApplication.QueuePlayerLoopUpdate();
                }
            }
        }

        void DrawVertexPathInspector()
        {

            GUILayout.Space(inspectorSectionSpacing);
            EditorGUILayout.LabelField("Vertex count: " + creator.BezierPath.NumPoints);
            GUILayout.Space(inspectorSectionSpacing);

            Data.showVertexPathOptions = EditorGUILayout.Foldout(Data.showVertexPathOptions, new GUIContent("Vertex Path Options"), true, boldFoldoutStyle);
            if (Data.showVertexPathOptions)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    Data.vertexPathMaxAngleError = EditorGUILayout.Slider(new GUIContent("Max Angle Error"), Data.vertexPathMaxAngleError, 0, 45);
                    Data.vertexPathMinVertexSpacing = EditorGUILayout.Slider(new GUIContent("Min Vertex Dst"), Data.vertexPathMinVertexSpacing, 0, 1);

                    GUILayout.Space(inspectorSectionSpacing);
                    if (check.changed)
                    {
                        Data.VertexPathSettingsChanged();
                        SceneView.RepaintAll();
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                }
            }

            Data.showVertexPathDisplayOptions = EditorGUILayout.Foldout(Data.showVertexPathDisplayOptions, new GUIContent("Display Options"), true, boldFoldoutStyle);
            if (Data.showVertexPathDisplayOptions)
            {
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    Data.showNormalsInVertexMode = GUILayout.Toggle(Data.showNormalsInVertexMode, new GUIContent("Show Normals"));
                    Data.showBezierPathInVertexMode = GUILayout.Toggle(Data.showBezierPathInVertexMode, new GUIContent("Show Bezier Path"));

                    if (check.changed)
                    {
                        SceneView.RepaintAll();
                        EditorApplication.QueuePlayerLoopUpdate();
                    }
                }
                DrawGlobalDisplaySettingsInspector();
            }
        }

        void DrawGlobalDisplaySettingsInspector()
        {
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                Data.globalDisplaySettingsFoldout = EditorGUILayout.InspectorTitlebar(Data.globalDisplaySettingsFoldout, GlobalDisplaySettings);
                if (Data.globalDisplaySettingsFoldout)
                {
                    CreateCachedEditor(GlobalDisplaySettings, null, ref globalDisplaySettingsEditor);
                    globalDisplaySettingsEditor.OnInspectorGUI();
                }
                if (check.changed)
                {
                    UpdateGlobalDisplaySettings();
                    SceneView.RepaintAll();
                }
            }
        }

        #endregion

        #region Scene GUI

        void OnSceneGUI()
        {
            if (!GlobalDisplaySettings.visibleBehindObjects)
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;

            EventType eventType = Event.current.type;

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                handlesStartCol = Handles.color;
                switch (Data.tabIndex)
                {
                    case bezierPathTab:
                        if (eventType != EventType.Repaint && eventType != EventType.Layout)
                            ProcessBezierPathInput(Event.current);

                        DrawBezierPathSceneEditor();
                        break;
                    case vertexPathTab:
                        if (eventType == EventType.Repaint)
                            DrawVertexPathSceneEditor();
                        break;
                }

                // Don't allow clicking over empty space to deselect the object
                if (eventType == EventType.Layout)
                    HandleUtility.AddDefaultControl(0);

                if (check.changed)
                    EditorApplication.QueuePlayerLoopUpdate();
            }

            SetTransformState();
        }

        void DrawVertexPathSceneEditor()
        {

            Color bezierCol = GlobalDisplaySettings.BezierPath;
            bezierCol.a *= .5f;

            if (Data.showBezierPathInVertexMode)
            {
                for (int i = 0; i < BezierPath.NumSegments; i++)
                {
                    Vector3[] points = BezierPath.GetPointsInSegment(i);
                    for (int j = 0; j < points.Length; j++)
                        points[j] = MathUtility.TransformPoint(points[j], creator.transform, BezierPath.Space);

                    Handles.DrawBezier(points[0], points[3], points[1], points[2], bezierCol, null, 2);
                }
            }

            Handles.color = GlobalDisplaySettings.vertexPath;

            for (int i = 0; i < creator.VertexPath.NumPoints; i++)
            {
                int nextIndex = (i + 1) % creator.VertexPath.NumPoints;
                if (nextIndex != 0 || BezierPath.IsClosed)
                    Handles.DrawLine(creator.VertexPath.GetPoint(i), creator.VertexPath.GetPoint(nextIndex));
            }

            if (Data.showNormalsInVertexMode)
            {
                Handles.color = GlobalDisplaySettings.normals;
                Vector3[] normalLines = new Vector3[creator.VertexPath.NumPoints * 2];
                for (int i = 0; i < creator.VertexPath.NumPoints; i++)
                {
                    normalLines[i * 2] = creator.VertexPath.GetPoint(i);
                    normalLines[i * 2 + 1] = creator.VertexPath.GetPoint(i) + creator.VertexPath.localNormals[i] * GlobalDisplaySettings.normalsLength;
                }
                Handles.DrawLines(normalLines);
            }
        }

        void ProcessBezierPathInput(Event e)
        {
            // Find which handle mouse is over. Start by looking at previous handle index first, as most likely to still be closest to mouse
            int previousMouseOverHandleIndex = (mouseOverHandleIndex == -1) ? 0 : mouseOverHandleIndex;
            mouseOverHandleIndex = -1;
            for (int i = 0; i < BezierPath.NumPoints; i += 3)
            {

                int handleIndex = (previousMouseOverHandleIndex + i) % BezierPath.NumPoints;
                float handleRadius = GetHandleDiameter(GlobalDisplaySettings.anchorSize * Data.bezierHandleScale, BezierPath[handleIndex]) / 2f;
                Vector3 pos = MathUtility.TransformPoint(BezierPath[handleIndex], creator.transform, BezierPath.Space);
                float dst = HandleUtility.DistanceToCircle(pos, handleRadius);
                if (dst == 0)
                {
                    mouseOverHandleIndex = handleIndex;
                    break;
                }
            }

            // Shift-left click (when mouse not over a handle) to split or add segment
            if (mouseOverHandleIndex == -1)
            {
                if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
                {
                    UpdatePathMouseInfo();
                    // Insert point along selected segment
                    if (selectedSegmentIndex != -1 && selectedSegmentIndex < BezierPath.NumSegments)
                    {
                        Vector3 newPathPoint = pathMouseInfo.closestWorldPointToMouse;
                        newPathPoint = MathUtility.InverseTransformPoint(newPathPoint, creator.transform, BezierPath.Space);
                        Undo.RecordObject(creator, "Split segment");
                        BezierPath.SplitSegment(newPathPoint, selectedSegmentIndex, pathMouseInfo.timeOnBezierSegment);
                    }
                    // If VertexPath is not a closed loop, add new point on to the end of the VertexPath
                    else if (!BezierPath.IsClosed)
                    {
                        // If control/command are held down, the point gets pre-pended, so we want to check distance
                        // to the endpoint we are adding to
                        var pointIdx = e.control || e.command ? 0 : BezierPath.NumPoints - 1;
                        // insert new point at same dst from scene camera as the point that comes before it (for a 3d VertexPath)
                        var endPointLocal = BezierPath[pointIdx];
                        var endPointGlobal =
                            MathUtility.TransformPoint(endPointLocal, creator.transform, BezierPath.Space);
                        var distanceCameraToEndpoint = (Camera.current.transform.position - endPointGlobal).magnitude;
                        var newPointGlobal =
                            MouseUtility.GetMouseWorldPosition(BezierPath.Space, distanceCameraToEndpoint);
                        var newPointLocal =
                            MathUtility.InverseTransformPoint(newPointGlobal, creator.transform, BezierPath.Space);

                        Undo.RecordObject(creator, "Add segment");
                        if (e.control || e.command)
                            BezierPath.AddSegmentToStart(newPointLocal);
                        else
                            BezierPath.AddSegmentToEnd(newPointLocal);

                    }

                }
            }

            // Control click or backspace/delete to remove point
            if (e.keyCode == KeyCode.Backspace || e.keyCode == KeyCode.Delete || ((e.control || e.command) && e.type == EventType.MouseDown && e.button == 0))
            {

                if (mouseOverHandleIndex != -1)
                {
                    Undo.RecordObject(creator, "Delete segment");
                    BezierPath.DeleteSegment(mouseOverHandleIndex);
                    if (mouseOverHandleIndex == handleIndexToDisplayAsTransform)
                        handleIndexToDisplayAsTransform = -1;

                    mouseOverHandleIndex = -1;
                    Repaint();
                }
            }

            // Holding shift and moving mouse (but mouse not over a handle/dragging a handle)
            if (draggingHandleIndex == -1 && mouseOverHandleIndex == -1)
            {
                bool shiftDown = e.shift && !shiftLastFrame;
                if (shiftDown || ((e.type == EventType.MouseMove || e.type == EventType.MouseDrag) && e.shift))
                {

                    UpdatePathMouseInfo();

                    if (pathMouseInfo.mouseDstToLine < segmentSelectDistanceThreshold)
                    {
                        if (pathMouseInfo.closestSegmentIndex != selectedSegmentIndex)
                        {
                            selectedSegmentIndex = pathMouseInfo.closestSegmentIndex;
                            HandleUtility.Repaint();
                        }
                    }
                    else
                    {
                        selectedSegmentIndex = -1;
                        HandleUtility.Repaint();
                    }

                }
            }

            shiftLastFrame = e.shift;

        }

        void DrawBezierPathSceneEditor()
        {

            bool displayControlPoints = Data.displayControlPoints && (BezierPath.ControlPointMode != BezierPath.ControlMode.Automatic || !GlobalDisplaySettings.hideAutoControls);
            Bounds bounds = BezierPath.CalculateBoundsWithTransform(creator.transform);

            if (Event.current.type == EventType.Repaint)
            {
                for (int i = 0; i < BezierPath.NumSegments; i++)
                {
                    Vector3[] points = BezierPath.GetPointsInSegment(i);
                    for (int j = 0; j < points.Length; j++)
                        points[j] = MathUtility.TransformPoint(points[j], creator.transform, BezierPath.Space);

                    if (Data.showPerSegmentBounds)
                    {
                        Bounds segmentBounds = PathUtility.CalculateSegmentBounds(points[0], points[1], points[2], points[3]);
                        Handles.color = GlobalDisplaySettings.segmentBounds;
                        Handles.DrawWireCube(segmentBounds.center, segmentBounds.size);
                    }

                    // Draw lines between control points
                    if (displayControlPoints)
                    {
                        Handles.color = (BezierPath.ControlPointMode == BezierPath.ControlMode.Automatic) ? GlobalDisplaySettings.handleDisabled : GlobalDisplaySettings.controlLine;
                        Handles.DrawLine(points[1], points[0]);
                        Handles.DrawLine(points[2], points[3]);
                    }

                    // Draw VertexPath
                    bool highlightSegment = (i == selectedSegmentIndex && Event.current.shift && draggingHandleIndex == -1 && mouseOverHandleIndex == -1);
                    Color segmentCol = (highlightSegment) ? GlobalDisplaySettings.highlightedPath : GlobalDisplaySettings.BezierPath;
                    Handles.DrawBezier(points[0], points[3], points[1], points[2], segmentCol, null, 2);
                }

                if (Data.showPathBounds)
                {
                    Handles.color = GlobalDisplaySettings.bounds;
                    Handles.DrawWireCube(bounds.center, bounds.size);
                }

                // Draw normals
                if (Data.showNormals)
                {
                    if (!hasUpdatedNormalsVertexPath)
                    {
                        normalsVertexPath = new VertexPath(BezierPath, creator.transform, normalsSpacing);
                        hasUpdatedNormalsVertexPath = true;
                    }

                    if (editingNormalsOld != Data.showNormals)
                    {
                        editingNormalsOld = Data.showNormals;
                        Repaint();
                    }

                    Vector3[] normalLines = new Vector3[normalsVertexPath.NumPoints * 2];
                    Handles.color = GlobalDisplaySettings.normals;
                    for (int i = 0; i < normalsVertexPath.NumPoints; i++)
                    {
                        normalLines[i * 2] = normalsVertexPath.GetPoint(i);
                        normalLines[i * 2 + 1] = normalsVertexPath.GetPoint(i) + normalsVertexPath.GetNormal(i) * GlobalDisplaySettings.normalsLength;
                    }
                    Handles.DrawLines(normalLines);
                }
            }

            if (Data.displayAnchorPoints)
            {
                for (int i = 0; i < BezierPath.NumPoints; i += 3)
                    DrawHandle(i);
            }
            if (displayControlPoints)
            {
                for (int i = 1; i < BezierPath.NumPoints - 1; i += 3)
                {
                    DrawHandle(i);
                    DrawHandle(i + 1);
                }
            }
        }

        void DrawHandle(int i)
        {
            Vector3 handlePosition = MathUtility.TransformPoint(BezierPath[i], creator.transform, BezierPath.Space);

            float anchorHandleSize = GetHandleDiameter(GlobalDisplaySettings.anchorSize * Data.bezierHandleScale, BezierPath[i]);
            float controlHandleSize = GetHandleDiameter(GlobalDisplaySettings.controlSize * Data.bezierHandleScale, BezierPath[i]);

            bool isAnchorPoint = i % 3 == 0;
            bool isInteractive = isAnchorPoint || BezierPath.ControlPointMode != BezierPath.ControlMode.Automatic;
            float handleSize = (isAnchorPoint) ? anchorHandleSize : controlHandleSize;
            bool doTransformHandle = i == handleIndexToDisplayAsTransform;

            PathHandle.HandleColours handleColours = (isAnchorPoint) ? splineAnchorColours : splineControlColours;
            if (i == handleIndexToDisplayAsTransform)
                handleColours.defaultColour = (isAnchorPoint) ? GlobalDisplaySettings.anchorSelected : GlobalDisplaySettings.controlSelected;

            var cap = capFunctions[(isAnchorPoint) ? GlobalDisplaySettings.anchorShape : GlobalDisplaySettings.controlShape];
            handlePosition = PathHandle.DrawHandle(handlePosition, BezierPath.Space, isInteractive, handleSize, cap, handleColours, out PathHandle.HandleInputType handleInputType, i);

            if (doTransformHandle)
            {
                // Show normals rotate tool 
                if (Data.showNormals && Tools.current == Tool.Rotate && isAnchorPoint && BezierPath.Space == PathSpace.xyz)
                {
                    Handles.color = handlesStartCol;

                    int attachedControlIndex = (i == BezierPath.NumPoints - 1) ? i - 1 : i + 1;
                    Vector3 dir = (BezierPath[attachedControlIndex] - handlePosition).normalized;
                    float handleRotOffset = (360 + BezierPath.GlobalNormalsAngle) % 360;
                    anchorAngleHandle.radius = handleSize * 3;
                    anchorAngleHandle.angle = handleRotOffset + BezierPath.GetAnchorNormalAngle(i / 3);
                    Vector3 handleDirection = Vector3.Cross(dir, Vector3.up);
                    Matrix4x4 handleMatrix = Matrix4x4.TRS(
                        handlePosition,
                        Quaternion.LookRotation(handleDirection, dir),
                        Vector3.one
                    );

                    using (new Handles.DrawingScope(handleMatrix))
                    {
                        // draw the handle
                        EditorGUI.BeginChangeCheck();
                        anchorAngleHandle.DrawHandle();
                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(creator, "Set angle");
                            BezierPath.SetAnchorNormalAngle(i / 3, anchorAngleHandle.angle - handleRotOffset);
                        }
                    }

                }
                else
                    handlePosition = Handles.DoPositionHandle(handlePosition, Quaternion.identity);

            }

            switch (handleInputType)
            {
                case PathHandle.HandleInputType.LMBDrag:
                    draggingHandleIndex = i;
                    handleIndexToDisplayAsTransform = -1;
                    Repaint();
                    break;
                case PathHandle.HandleInputType.LMBRelease:
                    draggingHandleIndex = -1;
                    handleIndexToDisplayAsTransform = -1;
                    Repaint();
                    break;
                case PathHandle.HandleInputType.LMBClick:
                    draggingHandleIndex = -1;
                    if (Event.current.shift)
                        handleIndexToDisplayAsTransform = -1; // disable move tool if new point added
                    else
                    {
                        if (handleIndexToDisplayAsTransform == i)
                            handleIndexToDisplayAsTransform = -1; // disable move tool if clicking on point under move tool
                        else
                            handleIndexToDisplayAsTransform = i;
                    }
                    Repaint();
                    break;
                case PathHandle.HandleInputType.LMBPress:
                    if (handleIndexToDisplayAsTransform != i)
                    {
                        handleIndexToDisplayAsTransform = -1;
                        Repaint();
                    }
                    break;
            }

            Vector3 localHandlePosition = MathUtility.InverseTransformPoint(handlePosition, creator.transform, BezierPath.Space);

            if (BezierPath[i] != localHandlePosition)
            {
                Undo.RecordObject(creator, "Move point");
                BezierPath.MovePoint(i, localHandlePosition);
            }
        }

        #endregion

        #region Internal methods

        void OnDisable()
        {
            Tools.hidden = false;
        }

        void OnEnable()
        {
            creator = (PathCreator)target;
            bool in2DEditorMode = EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D;
            creator.InitializeEditorData(in2DEditorMode);

            Data.bezierCreated -= ResetState;
            Data.bezierCreated += ResetState;
            Undo.undoRedoPerformed -= OnUndoRedo;
            Undo.undoRedoPerformed += OnUndoRedo;

            LoadDisplaySettings();
            UpdateGlobalDisplaySettings();
            ResetState();
            SetTransformState(true);
        }

        void SetTransformState(bool initialize = false)
        {
            Transform t = creator.transform;
            if (!initialize)
            {
                if (transformPos != t.position || t.localScale != transformScale || t.rotation != transformRot)
                    Data.PathTransformed();
            }
            transformPos = t.position;
            transformScale = t.localScale;
            transformRot = t.rotation;
        }

        void OnUndoRedo()
        {
            hasUpdatedScreenSpaceLine = false;
            hasUpdatedNormalsVertexPath = false;
            selectedSegmentIndex = -1;

            Repaint();
        }

        void TabChanged()
        {
            SceneView.RepaintAll();
            RepaintUnfocusedSceneViews();
        }

        void LoadDisplaySettings()
        {
            capFunctions = new Dictionary<GlobalDisplaySettings.HandleType, Handles.CapFunction>
            {
                { GlobalDisplaySettings.HandleType.Circle, Handles.CylinderHandleCap },
                { GlobalDisplaySettings.HandleType.Sphere, Handles.SphereHandleCap },
                { GlobalDisplaySettings.HandleType.Square, Handles.CubeHandleCap }
            };
        }

        void UpdateGlobalDisplaySettings()
        {
            var gds = GlobalDisplaySettings;
            splineAnchorColours = new PathHandle.HandleColours(gds.anchor, gds.anchorHighlighted, gds.anchorSelected, gds.handleDisabled);
            splineControlColours = new PathHandle.HandleColours(gds.control, gds.controlHighlighted, gds.controlSelected, gds.handleDisabled);

            anchorAngleHandle.fillColor = new Color(1, 1, 1, .05f);
            anchorAngleHandle.wireframeColor = Color.grey;
            anchorAngleHandle.radiusHandleColor = Color.clear;
            anchorAngleHandle.angleHandleColor = Color.white;
        }

        void ResetState()
        {
            selectedSegmentIndex = -1;
            draggingHandleIndex = -1;
            mouseOverHandleIndex = -1;
            handleIndexToDisplayAsTransform = -1;
            hasUpdatedScreenSpaceLine = false;
            hasUpdatedNormalsVertexPath = false;

            BezierPath.OnModified -= OnPathModifed;
            BezierPath.OnModified += OnPathModifed;

            SceneView.RepaintAll();
            EditorApplication.QueuePlayerLoopUpdate();
        }

        void OnPathModifed()
        {
            hasUpdatedScreenSpaceLine = false;
            hasUpdatedNormalsVertexPath = false;

            RepaintUnfocusedSceneViews();
        }

        void RepaintUnfocusedSceneViews()
        {
            // If multiple scene views are open, repaint those which do not have focus.
            if (SceneView.sceneViews.Count > 1)
            {
                foreach (SceneView sv in SceneView.sceneViews)
                {
                    if (EditorWindow.focusedWindow != sv)
                        sv.Repaint();
                }
            }
        }

        void UpdatePathMouseInfo()
        {
            if (!hasUpdatedScreenSpaceLine || (screenSpaceLine != null && screenSpaceLine.TransformIsOutOfDate()))
            {
                screenSpaceLine = new ScreenSpacePolyLine(BezierPath, creator.transform, screenPolylineMaxAngleError, screenPolylineMinVertexDst);
                hasUpdatedScreenSpaceLine = true;
            }
            pathMouseInfo = screenSpaceLine.CalculateMouseInfo();
        }

        float GetHandleDiameter(float diameter, Vector3 handlePosition)
        {
            float scaledDiameter = diameter * constantHandleScale;
            if (Data.keepConstantHandleSize)
                scaledDiameter *= HandleUtility.GetHandleSize(handlePosition) * 2.5f;

            return scaledDiameter;
        }

        BezierPath BezierPath { get { return Data.BezierPath; } }
        PathCreatorData Data { get { return creator.EditorData; } }
        bool EditingNormals { get { return Tools.current == Tool.Rotate && handleIndexToDisplayAsTransform % 3 == 0 && BezierPath.Space == PathSpace.xyz; } }

        #endregion

    }

}