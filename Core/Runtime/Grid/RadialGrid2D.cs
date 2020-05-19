using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Utility;
using TMPro;

namespace CoreScript.CustomGrids
{
    [System.Serializable]
    public class RadialGrid2D<TGridObject> : Grids<TGridObject>
    {
        protected new TGridObject[] grid;
        protected new TextMeshPro[] gridText;

        public new TGridObject[] Grid { get { return grid; } }
        public new TextMeshPro[] GridText { get { return gridText; } }

        //Width = radius
        //Height = angleSecPerRadiusSec
        //gridSize = radiusSize
        public RadialGrid2D(int radiusSec, int angleSecPerRadiusSec, float radiusSize, Vector3 originPos, Transform parent) : base(radiusSec, angleSecPerRadiusSec, radiusSize, originPos)
        {
            int total = 1;

            for (int i = 1; i < Width; i++)
                total += i * Height;

            Debug.Log("Length = " + total);

            grid = new TGridObject[total];
            gridText = new TextMeshPro[total];

            for (int currRadiusSec = 0; currRadiusSec < Width; ++currRadiusSec)
            {
                int numOfSec = currRadiusSec == 0 ? numOfSec = 1 : currRadiusSec * Height;

                int startOfSec = SectionToIndex(currRadiusSec, 1);

                float angleSize = 360 / numOfSec;
                for (int currAngleSec = 0; currAngleSec < numOfSec; ++currAngleSec)
                {
                    int index = startOfSec + currAngleSec;
                    grid[index] = default;
                    Vector3 gridPos = currRadiusSec == 0 ? gridPos = Vector3.zero : GridToWorldPos(currRadiusSec, currAngleSec);
                    gridText[index] = UtilityClass.CreateWorldText(grid[index].ToString(), Color.white, parent, gridPos, 10, TextAlignmentOptions.Center, 0);
                }
            }
        }

        int SectionToIndex(int radius, int angle)
        {
            Debug.Log("====SectionToIndex Start====");
            Debug.Log("radius = " + radius);
            if (radius < 0)
                return 0;

            int indexSec = radius * Height;
            Debug.Log("indexSec = " + indexSec);
            Debug.Log("Total = " + (indexSec + angle));
            Debug.Log("====SectionToIndex End====");
            return indexSec + angle;
        }

        public override Vector3 GridToWorldPos(int radius, int angle)
        {
            int totalAngleSec = radius * Height;
            float radAngle = (360f * angle / totalAngleSec) * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(radAngle), Mathf.Sin(radAngle)) * (radius * GridSize.x) + originPos;
        }

        public override void WorldPosToGrid(Vector3 position, out int radius, out int angle)
        {
            Vector3 relativePos = position - originPos;
            radius = Mathf.FloorToInt(relativePos.magnitude / gridSize.x);
            int totalAngleSec = radius == 0 ? 1 : radius * Height;
            float angleRad = Mathf.Atan2(relativePos.y, relativePos.x);
            Debug.Log("Angle Radian = " + angleRad);
            float angleDeg = angleRad * Mathf.Rad2Deg + 45f;
            if (angleDeg < 0f)
                angleDeg += 360f;
            Debug.Log("Angle Deg = " + angleDeg);
            angle = Mathf.FloorToInt((angleDeg / 360) * totalAngleSec);
            Debug.Log("Angle Result = " + angle);
        }

        public override void SetObject(int radius, int angle, TGridObject gridObject)
        {
            if (!InGrid(radius))
                return;

            grid[SectionToIndex(radius, angle)] = gridObject;
            gridText[SectionToIndex(radius, angle)].text = gridObject.ToString();
        }

        public override TGridObject GetObject(int radius, int angle)
        {
            if (!InGrid(radius))
                return default;

            return grid[SectionToIndex(radius, angle)];
        }

        public override bool InGrid(int radius, int angle = 0)
        {
            return radius > 0 && radius < Width;
        }

        public override void Clear()
        {
            for (int index = 0; index < grid.Length; index++)
            {
                grid[index] = default;
                TextMeshPro tm = gridText[index];
                if (tm == null)
                    continue;

                gridText[index] = null;
                Object go = tm.gameObject;
                Object.DestroyImmediate(go);
            }
        }
    }
}