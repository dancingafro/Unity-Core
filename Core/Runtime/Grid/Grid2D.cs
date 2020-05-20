using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Utility;
using TMPro;

namespace CoreScript.CustomGrids
{
    [System.Serializable]
    public class Grid2D<TGridObject> : Grids<TGridObject>
    {
        protected new TGridObject[,] grid;
        protected new TextMeshPro[,] gridText;

        public new TGridObject[,] Grid { get { return grid; } }
        public new TextMeshPro[,] GridText { get { return gridText; } }

        public Grid2D(int width, int height, float gridSize, Vector3 originPos, Transform parent) : base(width, height, gridSize, originPos)
        {
            grid = new TGridObject[Width, Height];
            gridText = new TextMeshPro[Width, Height];

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    grid[x, y] = default;
                    gridText[x, y] = UtilityClass.CreateWorldText(grid[x, y].ToString(), Color.white, parent, GridToWorldPos(x, y) + this.gridSize * .5f, 10, TextAlignmentOptions.Center, 0);
                    Debug.DrawLine(GridToWorldPos(x, y), GridToWorldPos(x + 1, y), Color.white, 100f);
                    Debug.DrawLine(GridToWorldPos(x, y), GridToWorldPos(x, y + 1), Color.white, 100f);
                }
            }

            Debug.DrawLine(GridToWorldPos(0, Height), GridToWorldPos(Width, Height), Color.white, 100f);
            Debug.DrawLine(GridToWorldPos(Width, 0), GridToWorldPos(Width, Height), Color.white, 100f);
        }

        public Grid2D(int width, int height, float gridWidthSize, float gridHeightSize, Vector3 originPos, Transform parent) : base(width, height, gridWidthSize, gridHeightSize, originPos)
        {
            grid = new TGridObject[Width, Height];
            gridText = new TextMeshPro[Width, Height];

            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    grid[x, y] = default;
                    gridText[x, y] = UtilityClass.CreateWorldText(grid[x, y].ToString(), Color.white, parent, GridToWorldPos(x, y) + gridSize * .5f, 10, TextAlignmentOptions.Center, 0);
                    Debug.DrawLine(GridToWorldPos(x, y), GridToWorldPos(x + 1, y), Color.white, 100f);
                    Debug.DrawLine(GridToWorldPos(x, y), GridToWorldPos(x, y + 1), Color.white, 100f);
                }
            }

            Debug.DrawLine(GridToWorldPos(0, Height), GridToWorldPos(Width, Height), Color.white, 100f);
            Debug.DrawLine(GridToWorldPos(Width, 0), GridToWorldPos(Width, Height), Color.white, 100f);
        }

        public override Vector3 GridToWorldPos(int x, int y)
        {
            return new Vector3(x * gridSize.x, y * gridSize.y) + originPos;
        }

        public override void WorldPosToGrid(Vector3 position, out int x, out int y)
        {
            Vector3 relativePos = (position - originPos);
            x = Mathf.FloorToInt(relativePos.x / gridSize.x);
            y = Mathf.FloorToInt(relativePos.y / gridSize.y);
        }

        public override void SetObject(int x, int y, TGridObject gridObject)
        {
            if (!InGrid(x, y))
                return;

            grid[x, y] = gridObject;
            gridText[x, y].text = gridObject.ToString();
        }

        public override TGridObject GetObject(int x, int y)
        {
            if (!InGrid(x, y))
                return default;

            return grid[x, y];
        }

        public bool InGrid(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public override void Clear()
        {
            for (int x = 0; x < diemention.x; x++)
            {
                for (int y = 0; y < diemention.y; y++)
                {
                    grid[x, y] = default;
                    TextMeshPro tm = gridText[x, y];
                    if (tm == null)
                        continue;

                    Object obj = tm.gameObject;
                    gridText[x, y] = null;
                    Object.DestroyImmediate(obj);
                }
            }
        }

        public bool GetOldData(Grid2D<TGridObject> grids)
        {
            if (diemention.x < grids.diemention.x || diemention.y < grids.diemention.y || grids.grid[0, 0].GetType() != grid[0, 0].GetType())
                return false;

            for (int x = 0; x < grids.diemention.x; x++)
            {
                for (int y = 0; y < grids.diemention.y; y++)
                {
                    grid[x, y] = grids.grid[x, y];
                    gridText[x, y] = grids.gridText[x, y];
                }
            }
            return true;
        }
    }

    public abstract class Grids<TGridObject>
    {
        #region Fields
        protected Vector3Int diemention;
        protected Vector3 gridSize;
        protected TGridObject[,,] grid;
        protected TextMeshPro[,,] gridText;
        protected Vector3 originPos;
        #endregion
        #region Getters
        public int Width { get { return diemention.x; } }
        public int Height { get { return diemention.y; } }
        public int Depth { get { return diemention.z; } }
        public Vector3Int Diemention { get { return diemention; } }
        public Vector3 GridSize { get { return gridSize; } }
        public TGridObject[,,] Grid { get { return grid; } }
        public TextMeshPro[,,] GridText { get { return gridText; } }
        #endregion
        #region Constructors
        public Grids(int width, int height, float gridSize, Vector3 originPos)
        {
            diemention = new Vector3Int(width, height, 0);
            this.gridSize = new Vector3(gridSize, gridSize, gridSize);
            this.originPos = originPos;
        }

        public Grids(int width, int height, int depth, float gridSize, Vector3 originPos)
        {
            diemention = new Vector3Int(width, height, depth);
            this.gridSize = new Vector3(gridSize, gridSize, gridSize);
            this.originPos = originPos;
        }

        public Grids(int width, int height, float gridWidthSize, float gridHeightSize, Vector3 originPos)
        {
            diemention = new Vector3Int(width, height, 0);
            gridSize = new Vector3(gridWidthSize, gridHeightSize);
            this.originPos = originPos;
        }

        public Grids(int width, int height, int depth, float gridWidthSize, float gridHeightSize, float gridDepthSize, Vector3 originPos)
        {
            diemention = new Vector3Int(width, height, depth);
            gridSize = new Vector3(gridWidthSize, gridHeightSize, gridDepthSize);
            this.originPos = originPos;
        }
        #endregion
        #region AbstractMethods
        public abstract Vector3 GridToWorldPos(int x, int y);
        public abstract void WorldPosToGrid(Vector3 position, out int x, out int y);
        public abstract void SetObject(int x, int y, TGridObject gridObject);
        public abstract TGridObject GetObject(int x, int y);
        #endregion
        #region PreDefineMethods
        public void SetObject(Vector3 position, TGridObject gridObject)
        {
            WorldPosToGrid(position, out int x, out int y);
            SetObject(x, y, gridObject);
        }

        public TGridObject GetObject(Vector3 position)
        {
            WorldPosToGrid(position, out int x, out int y);
            return GetObject(x, y);
        }

        public bool InGrid(int x, int y, int z) { return x >= 0 && x < diemention.x && y >= 0 && y < diemention.y && z >= 0 && z < diemention.z; }
        #endregion
        #region VirtualFunctions
        public virtual void Clear()
        {
            for (int x = 0; x < diemention.x; x++)
            {
                for (int y = 0; y < diemention.y; y++)
                {
                    for (int z = 0; z < diemention.z; z++)
                    {
                        grid[x, y, z] = default;
                        TextMeshPro tm = gridText[x, y, z];
                        if (tm == null)
                            continue;

                        Object obj = tm.gameObject;
                        gridText[x, y, z] = null;
                        Object.DestroyImmediate(obj);
                    }
                }
            }
        }
        public virtual bool GetOldData(Grids<TGridObject> grids)
        {
            if (diemention.x < grids.diemention.x || diemention.y < grids.diemention.y || diemention.z < grids.diemention.z || grids.grid[0, 0, 0].GetType() != grid[0, 0, 0].GetType())
                return false;

            for (int x = 0; x < grids.diemention.x; x++)
            {
                for (int y = 0; y < grids.diemention.y; y++)
                {
                    for (int z = 0; z < grids.diemention.z; z++)
                    {
                        grid[x, y, z] = grids.grid[x, y, z];
                        gridText[x, y, z] = grids.gridText[x, y, z];
                    }
                }
            }
            return true;
        }
        #endregion
    }
}