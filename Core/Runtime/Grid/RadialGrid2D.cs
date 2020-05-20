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

            grid = new TGridObject[total];
            gridText = new TextMeshPro[total];

            for (int currRadiusSec = 0; currRadiusSec < Width; ++currRadiusSec)
            {
                int numOfSec = currRadiusSec == 0 ? 1 : currRadiusSec * Height;

                int startOfSec = SectionToIndex(currRadiusSec, 1);

                for (int currAngleSec = 0; currAngleSec < numOfSec; ++currAngleSec)
                {
                    int index = startOfSec + currAngleSec;
                    grid[index] = default;
                    Vector3 gridPos = currRadiusSec == 0 ? gridPos = Vector3.zero + originPos : GridToWorldPos(currRadiusSec, currAngleSec);
                    int totalAngleSec = currRadiusSec * Height;
                    Vector3 CenterOffSet = Vector3.zero;
                    //if (currRadiusSec != 0)
                    //{
                    //    float onePortionOfAngle = 360f * 1 / totalAngleSec;
                    //    float tempDeg = (currAngleSec * onePortionOfAngle + onePortionOfAngle * .5f) * Mathf.Deg2Rad;
                    //    CenterOffSet = (new Vector3(Mathf.Cos(tempDeg), -Mathf.Sin(tempDeg)) * currRadiusSec * GridSize.x + originPos) - gridPos;
                    //}
                    gridText[index] = UtilityClass.CreateWorldText(grid[index].ToString(), Color.white, parent, gridPos + CenterOffSet, (int)gridSize.x * 10, TextAlignmentOptions.Center, 0);
                }
            }
        }

        int SectionToIndex(int radius, int angle)
        {
            if (radius == 0)
                return 0;
            int radiusSec = 0;
            for (int i = 0; i < radius; i++)
                radiusSec += i * Height;

            return radiusSec + angle;
        }

        public override Vector3 GridToWorldPos(int radius, int angle)
        {
            if (radius == 0)
                return Vector3.zero;

            int totalAngleSec = radius * Height;
            float radAngle = Mathf.Deg2Rad * (360f * ((float)angle / totalAngleSec) + 180f) % 360f;
            return new Vector3(Mathf.Cos(radAngle), Mathf.Sin(radAngle)) * radius * GridSize.x + originPos;
        }

        public override void WorldPosToGrid(Vector3 position, out int radius, out int angle)
        {
            Vector3 relativePos = position - originPos;
            radius = Mathf.FloorToInt(relativePos.magnitude / gridSize.x);
            int totalAngleSec = radius == 0 ? 1 : (radius - 1) * Height + Height;
            float angleDeg = Mathf.Atan2(relativePos.y, relativePos.x) * Mathf.Rad2Deg;
            if (angleDeg < 0f)
                angleDeg += 360f;
            angle = radius == 0 ? 0 : 1 + Mathf.FloorToInt(totalAngleSec * angleDeg / 360);
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

        public bool InGrid(int radius)
        {
            return radius > -1 && radius < Width;
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

        public void SetTextToIndex()
        {
            for (int i = 0; i < gridText.Length; i++)
            {
                gridText[i].text = i.ToString();
            }
        }

        public bool GetOldData(RadialGrid2D<TGridObject> grids)
        {
            if (diemention.x < grids.diemention.x || diemention.y != grids.diemention.y || grids.grid[0].GetType() != grid[0].GetType())
                return false;

            for (int index = 0; index < grids.grid.Length; index++)
            {
                grid[index] = grids.grid[index];
                gridText[index] = grids.gridText[index];
            }

            return true;
        }
    }
}