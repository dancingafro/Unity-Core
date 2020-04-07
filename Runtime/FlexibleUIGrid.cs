using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CoreScript.UI
{

    public class FlexibleUIGrid : LayoutGroup
    {
        public enum FitType
        {
            Uniform,
            Width,
            Height,
            FitRows,
            FitCols
        }
        [SerializeField] FitType fitType = FitType.Uniform;

        [SerializeField] int rows = 1, cols = 1;
        [SerializeField] Vector2 cellSize = Vector2.zero, spacing = Vector2.zero;

        [SerializeField] bool fitX = false, fitY = false;

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            CalculateRowsAndCols();

            cellSize.Set(fitX ? CalculateCellSize(rectTransform.rect.width, cols, spacing.x, padding.left, padding.right) : cellSize.x, fitY ? CalculateCellSize(rectTransform.rect.height, rows, spacing.y, padding.top, padding.bottom) : cellSize.y);

            int currCol = 0, currRow = 0;

            for (int i = 0; i < rectChildren.Count; i++)
            {
                currRow = i / cols;
                currCol = i % rows;

                SetChildAlongAxis(rectChildren[i], 0, CalculatePos(cellSize.x, currCol, spacing.x, padding.left), cellSize.x);
                SetChildAlongAxis(rectChildren[i], 1, CalculatePos(cellSize.y, currRow, spacing.y, padding.top), cellSize.y);
            }
        }

        float CalculateCellSize(float maxSize, int numberOfCell, float spacing, float paddingMin, float paddingMax)
        {
            return (maxSize / numberOfCell) - (spacing / numberOfCell * 2) - (paddingMin / numberOfCell) - (paddingMax / numberOfCell);
        }

        float CalculatePos(float cellSize, float currCell, float spacing, float padding)
        {
            return cellSize * currCell + spacing * currCell + padding;
        }

        void CalculateRowsAndCols()
        {
            switch (fitType)
            {
                case FitType.Uniform:
                case FitType.Width:
                case FitType.Height:
                    fitX = fitY = true;
                    rows = cols = Mathf.CeilToInt(Mathf.Sqrt(transform.childCount));
                    break;
            }

            switch (fitType)
            {
                case FitType.FitCols:
                case FitType.Width:
                    rows = CalculateCell(cols);
                    break;
                case FitType.FitRows:
                case FitType.Height:
                    cols = CalculateCell(rows);
                    break;
            }
        }

        int CalculateCell(float cell)
        {
            return Mathf.CeilToInt(transform.childCount / cell);
        }

        public override void CalculateLayoutInputVertical()
        {

        }

        public override void SetLayoutHorizontal()
        {

        }

        public override void SetLayoutVertical()
        {

        }
    }
}