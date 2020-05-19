using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Utility;

[System.Serializable]
public class CircularGrid2D<TGridObject>
{
    private int radiusSec, angleSecPerRadiusSec;
    private float radiusSize, angleSize;
    private TGridObject[] grid;
    private TextMesh[] gridText;
    private Vector3 originPos;

    public CircularGrid2D(int radiusSec, int angleSecPerRadiusSec, float radiusSize, float angleSize, Vector3 originPos)
    {
        this.radiusSec = radiusSec;
        this.angleSecPerRadiusSec = angleSecPerRadiusSec;
        this.radiusSize = radiusSize;
        this.angleSize = angleSize;
        this.originPos = originPos;

        int total = SectionToIndex(radiusSec - 1, radiusSec * angleSecPerRadiusSec) + 1;

        grid = new TGridObject[total];
        gridText = new TextMesh[total];

        for (int currRadiusSec = 0; currRadiusSec < radiusSec; currRadiusSec++)
        {
            int angleSec = currRadiusSec * angleSecPerRadiusSec;
            for (int currAngleSec = (currRadiusSec == 0) ? 0 : ((currRadiusSec - 1) * angleSecPerRadiusSec + 1); currAngleSec < angleSec + 1; currAngleSec++)
            {
                int index = SectionToIndex(currRadiusSec, currAngleSec);
                grid[index] = default;
                Vector3 gridPos = GridToWorldPos(currRadiusSec, currAngleSec);
                gridText[index] = UtilityClass.CreateWorldText(grid[index].ToString(), Color.white, null, gridPos + new Vector3(radiusSize, angleSize) * .5f, 10, TextAnchor.MiddleCenter, TextAlignment.Center, 0);
            }
        }
    }

    int SectionToIndex(int radius, int angle)
    {
        if (radius == 0)
            return 0;

        int indexSec = 0;

        for (int i = 0; i < radius; i++)
            indexSec += i * angleSecPerRadiusSec;

        return indexSec + angle;
    }

    public Vector3 GridToWorldPos(int radius, int angle)
    {
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * radius + originPos;
    }

    void WorldPosToGrid(Vector3 position, out int radius, out int angle)
    {
        Vector3 relativePos = position - originPos;
        radius = Mathf.FloorToInt(relativePos.magnitude / radiusSize);
        angle = Mathf.FloorToInt(Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg);
    }

    public void SetObject(int radius, int angle, TGridObject gridObject)
    {
        if (!InGrid(radius))
            return;

        grid[SectionToIndex(radius, angle)] = gridObject;
    }

    public void SetObject(Vector3 position, TGridObject gridObject)
    {
        WorldPosToGrid(position, out int radius, out int angle);
        SetObject(radius, angle, gridObject);
    }

    public TGridObject GetObject(int radius, int angle)
    {
        if (!InGrid(radius))
            return default;

        return grid[SectionToIndex(radius, angle)];
    }

    public TGridObject GetObject(Vector3 position)
    {
        WorldPosToGrid(position, out int radius, out int angle);
        return GetObject(radius, angle);
    }

    bool InGrid(int radius)
    {
        return radius < radiusSec;
    }
}
