using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    [SerializeField, HideInInspector] List<Vector2> points;
    [SerializeField, HideInInspector] bool isClosed;
    [SerializeField, HideInInspector] bool autoSetControlPoints;

    public Path(Vector2 centre)
    {
        points = new List<Vector2>()
        {
            centre + Vector2.left,
            centre + Vector2.left * Vector2.up * .5f,
            centre + Vector2.right * Vector2.down * .5f,
            centre + Vector2.right
        };
    }

    public Vector2 this[int i] { get { return points[i]; } }
    public int NumPoints { get { return points.Count; } }
    public int NumOfSegments { get { return points.Count / 3; } }

    public bool AutoSetControlPoints
    {
        get
        {
            return autoSetControlPoints;
        }
        set
        {
            autoSetControlPoints = value;
            if (autoSetControlPoints)
                AutoSetAllControlPoints();
        }
    }

    public void AddSegment(Vector2 anchorPos)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * .5f);
        points.Add(anchorPos);

        if (AutoSetControlPoints)
            AutoSetAllAffectedControlPoints(points.Count - 1);

    }

    public Vector2[] GetPointsInSegment(int i)
    {
        return new Vector2[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)] };
    }

    public void MovePoint(int i, Vector2 pos)
    {
        Vector2 deltaMove = pos - points[i];
        //if (i % 3 != 0 && AutoSetControlPoints)
        //    return;

        points[i] = pos;
        if (AutoSetControlPoints)
        {
            AutoSetAllAffectedControlPoints(i);
            return;
        }

        if (i % 3 == 0)
        {
            if (isClosed || i + 1 < points.Count)
                points[LoopIndex(i + 1)] += deltaMove;
            if (i - 1 > -1)
                points[i - 1] += deltaMove;
            return;
        }

        GetCorrespondingIndex((i + 1) % 3 == 0, i, out int correspondingControlIndex, out int anchorPointIndex);

        if (correspondingControlIndex < 0 && correspondingControlIndex > points.Count && !isClosed)
            return;

        float dst = (points[anchorPointIndex] - points[correspondingControlIndex]).magnitude;
        Vector2 dir = (points[anchorPointIndex] - pos).normalized;
        points[correspondingControlIndex] = points[anchorPointIndex] + dir * dst;
    }

    void GetCorrespondingIndex(bool nextPointIsAnchor, int i, out int correspondingControlIndex, out int anchorPointIndex)
    {
        correspondingControlIndex = LoopIndex(nextPointIsAnchor ? i + 2 : i - 2);
        anchorPointIndex = LoopIndex(nextPointIsAnchor ? i + 2 : i - 2);
    }

    public void ToggleClosed()
    {
        isClosed = !isClosed;
        if (!isClosed)
        {
            points.RemoveRange(points.Count - 2, 2);
            if (AutoSetControlPoints)
                AutoSetStartAndEndControls();
            return;
        }

        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add(points[0] * 2 - points[1]);
        if (!AutoSetControlPoints)
            return;

        AutoSetAnchorControlPoints(0);
        AutoSetAnchorControlPoints(points.Count - 3);
    }

    void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (i > -1 && i < points.Count || isClosed)
                AutoSetAnchorControlPoints(LoopIndex(i));
        }
        AutoSetStartAndEndControls();
    }

    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; ++i)
            AutoSetAnchorControlPoints(i);

        AutoSetStartAndEndControls();
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector2 anchorPos = points[anchorIndex];
        Vector2 dir = Vector2.zero;
        float[] neighbourDst = new float[2];
        if (anchorIndex - 3 > -1 || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDst[0] = offset.magnitude;
        }

        if (anchorIndex + 3 < points.Count || isClosed)
        {
            Vector2 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDst[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; ++i)
        {
            int controlIndex = LoopIndex(anchorIndex + i * 2 - 1);

            if (controlIndex > -1 && controlIndex < points.Count || isClosed)
                points[controlIndex] = anchorPos + dir * neighbourDst[i] * .5f;
        }
    }

    void AutoSetStartAndEndControls()
    {
        if (isClosed)
            return;

        points[1] = (points[0] + points[2]) * .5f;
        points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f;
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
}