using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public struct OrientedPoint
    {
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }

        public OrientedPoint(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 LocalToWorld(Vector3 point)
        {
            return Position + Rotation * point;
        }

        public Vector3 WorldToLocal(Vector3 point)
        {
            return Quaternion.Inverse(Rotation) * (point - Position);
        }

        public Vector3 LocalToWorldDirection(Vector3 dir)
        {
            return Rotation * dir;
        }
    }

    [System.Serializable]
    public class Path
    {
        public enum ControlModeOption { Aligned, Mirrored, Free, Automatic };

        [SerializeField, HideInInspector]
        List<OrientedPoint> points;
        [SerializeField, HideInInspector]
        bool isClosed;
        [SerializeField, HideInInspector]
        ControlModeOption controlMode;

        public Path(Vector3 centre, Quaternion rotation)
        {
            points = new List<OrientedPoint>
            {
               new OrientedPoint( centre + Vector3.left,rotation),
                new OrientedPoint( centre + (Vector3.left + Vector3.up) * .5f,rotation),
               new OrientedPoint(  centre + (Vector3.right + Vector3.down) * .5f,rotation),
               new OrientedPoint(  centre + Vector3.right,rotation)
            };
        }

        public OrientedPoint this[int i] { get { return points[i]; } }

        public bool IsClosed
        {
            get { return isClosed; }
            set
            {
                if (isClosed == value)
                    return;
                isClosed = value;

                if (isClosed)
                {
                    points.Add(new OrientedPoint(points[NumPoints - 1].Position * 2 - points[NumPoints - 2].Position, Quaternion.identity));
                    points.Add(new OrientedPoint(points[0].Position * 2 - points[1].Position, Quaternion.identity));
                    if (controlMode == ControlModeOption.Automatic)
                    {
                        AutoSetAnchorControlPoints(0);
                        AutoSetAnchorControlPoints(NumPoints - 3);
                    }
                    return;
                }

                points.RemoveRange(NumPoints - 2, 2);
                if (controlMode == ControlModeOption.Automatic)
                    AutoSetStartAndEndControls();
            }
        }
        public ControlModeOption ControlMode
        {
            get { return controlMode; }
            set
            {
                if (controlMode == value)
                    return;

                controlMode = value;
                ControlModeOptionChange();
            }
        }

        public int NumPoints { get { return points.Count; } }
        public int NumSegments { get { return points.Count / 3; } }

        public void AddSegment(Vector3 anchorPos)
        {
            points.Add(new OrientedPoint(points[NumPoints - 1].Position * 2 - points[NumPoints - 2].Position, Quaternion.identity));
            points.Add(new OrientedPoint((points[NumPoints - 1].Position + anchorPos) * .5f, Quaternion.identity));
            points.Add(new OrientedPoint(anchorPos, Quaternion.identity));
            ControlModeOptionChange();
        }

        public void SplitSegment(Vector3 anchorPos, int segmentIndex)
        {
            points.InsertRange(segmentIndex * 3 + 2, new OrientedPoint[] { new OrientedPoint(Vector3.zero, Quaternion.identity), new OrientedPoint(anchorPos, Quaternion.identity), new OrientedPoint(Vector3.zero, Quaternion.identity) });
            ControlModeOptionChange();
        }

        public void DeleteSegment(int anchorIndex)
        {
            if (NumSegments < 2)
                return;

            if (anchorIndex == 0)
            {
                if (isClosed)
                    points[NumPoints - 1] = points[2];

                points.RemoveRange(0, 3);
            }
            else if (!isClosed && anchorIndex == NumPoints - 1)
                points.RemoveRange(anchorIndex - 2, 3);
            else
                points.RemoveRange(anchorIndex - 1, 3);

        }

        public OrientedPoint[] GetPointsInSegment(int i)
        {
            return new OrientedPoint[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)] };
        }

        public void MovePoint(int i, Vector3 pos)
        {
            Vector3 deltaMove = pos - points[i].Position;
            OrientedPoint currPoint = points[i];
            switch (ControlMode)
            {
                case ControlModeOption.Aligned:
                    currPoint.Position = pos;
                    AlignedAffectedControlPoint(i, deltaMove);
                    break;
                case ControlModeOption.Mirrored:
                    currPoint.Position = pos;
                    MirrorAffectedControlPoint(i, deltaMove);
                    break;
                case ControlModeOption.Automatic:
                    if (i % 3 == 0)
                    {
                        currPoint.Position = pos;
                        AutoSetAllAffectedControlPoints(i);
                    }
                    break;
                default:
                    currPoint.Position = pos;
                    break;
            }
        }

        public OrientedPoint[] CalculateEvenlySpacedPoints(float spacing, float res = 1f)
        {
            OrientedPoint[] pts = GetPointsInSegment(0);
            List<OrientedPoint> orientedPoints = new List<OrientedPoint>()
            {
                points[0]
            };

            Vector3 previousPoint = points[0].Position;
            float dstSinceLastEvenPoint = 0, totalDstTravel = 0;
            for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
            {
                pts = GetPointsInSegment(segmentIndex);
                Vector3[] ptsPos = new Vector3[] { pts[0].Position, pts[1].Position, pts[2].Position, pts[3].Position };
                float controlNetLength = (pts[0].Position - pts[1].Position).magnitude + (pts[1].Position - pts[2].Position).magnitude + (pts[2].Position - pts[3].Position).magnitude;
                float estimatedCurveLegth = (pts[0].Position - pts[3].Position).magnitude + controlNetLength * .5f;
                int division = Mathf.CeilToInt(estimatedCurveLegth * res * 10);
                float t = 0, deltaT = 1f / division;
                while (t <= 1)
                {
                    t += deltaT;
                    Vector3 pointOnCurve = UtilityCode.CubicBezier(ptsPos, t);
                    float dstBetweenPoints = (previousPoint - pointOnCurve).magnitude;
                    dstSinceLastEvenPoint += dstBetweenPoints;
                    totalDstTravel += dstBetweenPoints;
                    while (dstSinceLastEvenPoint >= spacing)
                    {
                        float overShootDst = dstSinceLastEvenPoint - spacing;
                        dstSinceLastEvenPoint = overShootDst;
                        Vector3 newEvenlySpacePoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * spacing;
                        orientedPoints.Add(new OrientedPoint(newEvenlySpacePoint, UtilityCode.CubicOrientation(ptsPos, Vector3.up, t - 1 - overShootDst / totalDstTravel)));
                        previousPoint = newEvenlySpacePoint;
                    }

                    previousPoint = pointOnCurve;
                }
            }

            return orientedPoints.ToArray();
        }

        void ControlModeOptionChange()
        {
            switch (ControlMode)
            {
                case ControlModeOption.Aligned:
                    AlignedAllPoint();
                    break;
                case ControlModeOption.Mirrored:
                    MirrorAllPoint();
                    break;
                case ControlModeOption.Automatic:
                    AutoSetAllControlPoints();
                    break;
            }
        }

        void AlignedAllPoint()
        {
            for (int i = 0; i < NumPoints; i++)
                AlignedAffectedControlPoint(i, Vector3.zero);
        }

        void AlignedAffectedControlPoint(int index, Vector3 deltaMove)
        {
            if (index % 3 == 0)
            {
                OrientedPoint currPoint;
                if (index + 1 < NumPoints || isClosed)
                {
                    currPoint = points[LoopIndex(index + 1)];
                    currPoint.Position += deltaMove;
                }
                if (index - 1 >= 0 || isClosed)
                {
                    currPoint = points[LoopIndex(index - 1)];
                    currPoint.Position += deltaMove;
                }
                return;
            }

            bool nextPointIsAnchor = (index + 1) % 3 == 0;
            int correspondingControlIndex = (nextPointIsAnchor) ? index + 2 : index - 2;
            int anchorIndex = (nextPointIsAnchor) ? index + 1 : index - 1;

            if (correspondingControlIndex >= 0 && correspondingControlIndex < NumPoints || isClosed)
            {
                float dst = (points[LoopIndex(anchorIndex)].Position - points[LoopIndex(correspondingControlIndex)].Position).magnitude;
                Vector3 dir = (points[LoopIndex(anchorIndex)].Position - points[index].Position).normalized;
                OrientedPoint correspondingAnchor = points[LoopIndex(correspondingControlIndex)];
                correspondingAnchor.Position = points[LoopIndex(anchorIndex)].Position + dir * dst;
            }
        }

        void MirrorAllPoint()
        {
            for (int i = 1; i < NumPoints; i += 3)
                MirrorAffectedControlPoint(i, Vector3.zero);
        }

        void MirrorAffectedControlPoint(int index, Vector3 deltaMove)
        {
            if (index % 3 == 0)
            {
                OrientedPoint currPoint;
                if (index + 1 < NumPoints || isClosed)
                {
                    currPoint = points[LoopIndex(index + 1)];
                    currPoint.Position += deltaMove;
                }
                if (index - 1 >= 0 || isClosed)
                {
                    currPoint = points[LoopIndex(index - 1)];
                    currPoint.Position += deltaMove;
                }
                return;
            }

            bool nextPointIsAnchor = (index + 1) % 3 == 0;
            int correspondingControlIndex = (nextPointIsAnchor) ? index + 2 : index - 2;
            int anchorIndex = (nextPointIsAnchor) ? index + 1 : index - 1;

            if (correspondingControlIndex >= 0 && correspondingControlIndex < NumPoints || isClosed)
            {
                float dst = (points[LoopIndex(anchorIndex)].Position - points[index].Position).magnitude;
                Vector3 dir = (points[LoopIndex(anchorIndex)].Position - points[index].Position).normalized;
                OrientedPoint correspondingAnchor = points[LoopIndex(correspondingControlIndex)];
                correspondingAnchor.Position = points[LoopIndex(anchorIndex)].Position - dir * dst;
            }
        }

        void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
        {
            for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
            {
                if (i >= 0 && i < NumPoints || isClosed)
                    AutoSetAnchorControlPoints(LoopIndex(i));
            }

            AutoSetStartAndEndControls();
        }

        void AutoSetAllControlPoints()
        {
            for (int i = 0; i < NumPoints; i += 3)
                AutoSetAnchorControlPoints(i);

            AutoSetStartAndEndControls();
        }

        void AutoSetAnchorControlPoints(int anchorIndex)
        {
            OrientedPoint anchorOrientedPoint = points[anchorIndex];
            Vector3 dir = Vector3.zero;
            float[] neighbourDistances = new float[2];

            if (anchorIndex - 3 >= 0 || isClosed)
            {
                Vector3 offset = points[LoopIndex(anchorIndex - 3)].Position - anchorOrientedPoint.Position;
                dir += offset.normalized;
                neighbourDistances[0] = offset.magnitude;
            }
            if (anchorIndex + 3 >= 0 || isClosed)
            {
                Vector3 offset = points[LoopIndex(anchorIndex + 3)].Position - anchorOrientedPoint.Position;
                dir -= offset.normalized;
                neighbourDistances[1] = -offset.magnitude;
            }

            dir.Normalize();

            for (int i = 0; i < 2; i++)
            {
                int controlIndex = anchorIndex + i * 2 - 1;
                if (controlIndex >= 0 && controlIndex < NumPoints || isClosed)
                {
                    OrientedPoint point = points[LoopIndex(controlIndex)];
                    point.Position = anchorOrientedPoint.Position + dir * neighbourDistances[i] * .5f;
                }
            }
        }

        void AutoSetStartAndEndControls()
        {
            if (isClosed)
                return;
            OrientedPoint point = points[1];

            point.Position = (points[0].Position + points[2].Position) * .5f;
            point = points[NumPoints - 2];
            point.Position = (points[NumPoints - 1].Position + points[NumPoints - 3].Position) * .5f;
        }

        int LoopIndex(int i)
        {
            return (i + NumPoints) % NumPoints;
        }
    }

}