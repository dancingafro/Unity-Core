using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Utility;

[System.Serializable]
public class Grid2D<TGridObject>
{
    private readonly int width, height;
    private readonly float gridSize;
    private TGridObject[,] grid;
    private TextMesh[,] gridText;
    private Vector3 originPos;

    public Grid2D(int width, int height, float gridSize, Vector3 originPos)
    {
        this.width = width;
        this.height = height;
        this.gridSize = gridSize;
        this.originPos = originPos;

        grid = new TGridObject[width, height];
        gridText = new TextMesh[width, height];

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                grid[x, y] = default;
                gridText[x, y] = UtilityClass.CreateWorldText(grid[x, y].ToString(), Color.white, null, GridToWorldPos(x, y) + new Vector3(gridSize, gridSize) * .5f, 10, TextAnchor.MiddleCenter, TextAlignment.Center, 0);
                Debug.DrawLine(GridToWorldPos(x, y), GridToWorldPos(x + 1, y), Color.white, 100f);
                Debug.DrawLine(GridToWorldPos(x, y), GridToWorldPos(x, y + 1), Color.white, 100f);
            }
        }

        Debug.DrawLine(GridToWorldPos(0, height), GridToWorldPos(width, height), Color.white, 100f);
        Debug.DrawLine(GridToWorldPos(width, 0), GridToWorldPos(width, height), Color.white, 100f);
    }

    public Vector3 GridToWorldPos(int x, int y)
    {
        return new Vector3(x, y) * gridSize + originPos;
    }

    void WorldPosToGrid(Vector3 position, out int x, out int y)
    {
        x = Mathf.FloorToInt((position - originPos).x / gridSize);
        y = Mathf.FloorToInt((position - originPos).y / gridSize);
    }

    public void SetObject(int x, int y, TGridObject gridObject)
    {
        if (!InGrid(x, y))
            return;

        grid[x, y] = gridObject;
    }

    public void SetObject(Vector3 position, TGridObject gridObject)
    {
        WorldPosToGrid(position, out int x, out int y);
        SetObject(x, y, gridObject);
    }

    public TGridObject GetObject(int x, int y)
    {
        if (!InGrid(x, y))
            return default;

        return grid[x, y];
    }

    public TGridObject GetObject(Vector3 position)
    {
        WorldPosToGrid(position, out int x, out int y);
        return GetObject(x, y);
    }

    bool InGrid(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}
