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

        public Grid2D(int width, int height, float gridSize, Vector3 originPos, PositionSpace positionSpace, Transform parent) : base(width, height, gridSize, originPos, positionSpace)
        {
            grid = new TGridObject[Width, Height];
            gridText = new TextMeshPro[Width, Height];

            for (int horizontal = 0; horizontal < Width; ++horizontal)
            {
                for (int vertical = 0; vertical < Height; ++vertical)
                {
                    grid[horizontal, vertical] = default;
                    gridText[horizontal, vertical] = UtilityClass.CreateWorldText(grid[horizontal, vertical].ToString(), Color.white, parent, GridToWorldPos(horizontal, vertical) + this.gridSize * .5f, 10, TextAlignmentOptions.Center, 0);
                    Debug.DrawLine(GridToWorldPos(horizontal, vertical), GridToWorldPos(horizontal + 1, vertical), Color.white, 100f);
                    Debug.DrawLine(GridToWorldPos(horizontal, vertical), GridToWorldPos(horizontal, vertical + 1), Color.white, 100f);
                }
            }

            Debug.DrawLine(GridToWorldPos(0, Height), GridToWorldPos(Width, Height), Color.white, 100f);
            Debug.DrawLine(GridToWorldPos(Width, 0), GridToWorldPos(Width, Height), Color.white, 100f);
        }

        public Grid2D(int width, int height, float gridWidthSize, float gridHeightSize, Vector3 originPos, PositionSpace positionSpace, Transform parent) : base(width, height, gridWidthSize, gridHeightSize, originPos, positionSpace)
        {
            grid = new TGridObject[Width, Height];
            gridText = new TextMeshPro[Width, Height];

            for (int horizontal = 0; horizontal < Width; ++horizontal)
            {
                for (int vertical = 0; vertical < Height; ++vertical)
                {
                    grid[horizontal, vertical] = default;
                    Vector3 gridPos = GridToWorldPos(horizontal, vertical);
                    gridText[horizontal, vertical] = UtilityClass.CreateWorldText(grid[horizontal, vertical].ToString(), Color.white, parent, gridPos + CenterOffset, 10, TextAlignmentOptions.Center, 0);
                    Debug.DrawLine(gridPos, GridToWorldPos(horizontal + 1, vertical), Color.white, 100f);
                    Debug.DrawLine(gridPos, GridToWorldPos(horizontal, vertical + 1), Color.white, 100f);
                }
            }

            Debug.DrawLine(GridToWorldPos(0, Height), GridToWorldPos(Width, Height), Color.white, 100f);
            Debug.DrawLine(GridToWorldPos(Width, 0), GridToWorldPos(Width, Height), Color.white, 100f);
        }

        public override Vector3 GridToWorldPos(int horizontal, int vertical)
        {
            Vector3 worldPos;

            switch (positionSpace)
            {
                case PositionSpace.xz:
                    worldPos = new Vector3(horizontal * Width, 0, vertical * Height);
                    break;
                case PositionSpace.yz:
                    worldPos = new Vector3(0, horizontal * Width, vertical * Height);
                    break;
                default:
                    worldPos = new Vector3(horizontal * Width, vertical * Height);
                    break;
            }

            return worldPos + originPos;
        }

        public override void WorldPosToGrid(Vector3 position, out int horizontal, out int Vertical)
        {
            Vector3 relativePos = position - originPos;
            switch (positionSpace)
            {
                case PositionSpace.xz:
                    horizontal = Mathf.FloorToInt(relativePos.x / gridSize.x);
                    Vertical = Mathf.FloorToInt(relativePos.z / gridSize.z);
                    return;
                case PositionSpace.yz:
                    horizontal = Mathf.FloorToInt(relativePos.y / gridSize.y);
                    Vertical = Mathf.FloorToInt(relativePos.z / gridSize.z);
                    return;
            }
            horizontal = Mathf.FloorToInt(relativePos.x / gridSize.x);
            Vertical = Mathf.FloorToInt(relativePos.y / gridSize.y);
        }

        public override void SetObject(int horizontal, int vertical, TGridObject gridObject)
        {
            if (!InGrid(horizontal, vertical))
                return;

            grid[horizontal, vertical] = gridObject;
            gridText[horizontal, vertical].text = gridObject.ToString();
        }

        public override TGridObject GetObject(int horizontal, int vertical)
        {
            if (!InGrid(horizontal, vertical))
                return default;

            return grid[horizontal, vertical];
        }

        public bool InGrid(int horizontal, int vertical)
        {
            return horizontal >= 0 && horizontal < Width && vertical >= 0 && vertical < Height;
        }

        public override void Clear()
        {
            for (int horizontal = 0; horizontal < Width; horizontal++)
            {
                for (int vertical = 0; vertical < Height; vertical++)
                {
                    grid[horizontal, vertical] = default;
                    TextMeshPro tm = gridText[horizontal, vertical];
                    if (tm == null)
                        continue;

                    Object obj = tm.gameObject;
                    gridText[horizontal, vertical] = null;
                    Object.DestroyImmediate(obj);
                }
            }
        }

        public bool GetOldData(Grid2D<TGridObject> grids)
        {
            if (Width < grids.Width || Height < grids.Height || grids.grid[0, 0].GetType() != grid[0, 0].GetType())
                return false;

            for (int horizontal = 0; horizontal < grids.Width; horizontal++)
            {
                for (int veritcal = 0; veritcal < grids.Height; veritcal++)
                {
                    grid[horizontal, veritcal] = grids.grid[horizontal, veritcal];
                    gridText[horizontal, veritcal] = grids.gridText[horizontal, veritcal];
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
        protected PositionSpace positionSpace;
        #endregion
        #region Getters
        public int Width
        {
            get
            {
                switch (positionSpace)
                {
                    case PositionSpace.yz:
                        return diemention.y;
                }
                return diemention.x;
            }
        }
        public int Height
        {
            get
            {
                switch (positionSpace)
                {
                    case PositionSpace.xy:
                        return diemention.y;
                }
                return diemention.z;
            }
        }
        public int Depth { get { return diemention.z; } }
        public Vector3Int Diemention { get { return diemention; } }
        public Vector3 GridSize { get { return gridSize; } }
        public PositionSpace PositionSpace { get { return positionSpace; } }
        public TGridObject[,,] Grid { get { return grid; } }
        public TextMeshPro[,,] GridText { get { return gridText; } }
        public virtual Vector3 CenterOffset { get { return gridSize * .5f; } }
        #endregion
        #region Constructors
        public Grids(int width, int height, float gridSize, Vector3 originPos, PositionSpace positionSpace)
        {
            this.positionSpace = positionSpace;
            this.originPos = originPos;
            switch (positionSpace)
            {
                case PositionSpace.xyz:
                case PositionSpace.xy:
                    diemention = new Vector3Int(width, height, 0);
                    this.gridSize = new Vector3(gridSize, gridSize);
                    break;
                case PositionSpace.xz:
                    diemention = new Vector3Int(width, 0, height);
                    this.gridSize = new Vector3(gridSize, 0, gridSize);
                    break;
                case PositionSpace.yz:
                    diemention = new Vector3Int(0, width, height);
                    this.gridSize = new Vector3(0, gridSize, gridSize);
                    break;
            }
        }

        public Grids(int width, int height, float gridWidthSize, float gridHeightSize, Vector3 originPos, PositionSpace positionSpace)
        {
            this.positionSpace = positionSpace;
            this.originPos = originPos;
            switch (positionSpace)
            {
                case PositionSpace.xyz:
                case PositionSpace.xy:
                    diemention = new Vector3Int(width, height, 0);
                    gridSize = new Vector3(gridWidthSize, gridHeightSize);
                    break;
                case PositionSpace.xz:
                    diemention = new Vector3Int(width, 0, height);
                    gridSize = new Vector3(gridWidthSize, 0, gridHeightSize);
                    break;
                case PositionSpace.yz:
                    diemention = new Vector3Int(0, width, height);
                    gridSize = new Vector3(0, gridWidthSize, gridHeightSize);
                    break;
            }
        }

        public Grids(int width, int height, int depth, float gridSize, Vector3 originPos)
        {
            positionSpace = PositionSpace.xyz;
            diemention = new Vector3Int(width, height, depth);
            this.gridSize = new Vector3(gridSize, gridSize, gridSize);
            this.originPos = originPos;
        }

        public Grids(int width, int height, int depth, float gridWidthSize, float gridHeightSize, float gridDepthSize, Vector3 originPos)
        {
            positionSpace = PositionSpace.xyz;
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