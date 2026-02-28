using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.LevelSystem
{
    public static class ConveyorScanMapper
    {
        // Grid space: node localPositions are in grid units (before cellSize multiply).
        // 0.5f offset = half a cell, placing nodes at cell centers or just outside the grid edge.
        private const float HalfCell = 0.5f;

        public static ConveyorPathData GeneratePath(int gridW, int gridH)
        {
            var data = ScriptableObject.CreateInstance<ConveyorPathData>();
            data.gridWidth = gridW;
            data.gridHeight = gridH;

            var nodes = new List<ConveyorNode>();
            int idx = 0;

            float left = -HalfCell;
            float right = gridW + HalfCell;
            float bottom = -HalfCell;
            float top = gridH + HalfCell;

            int roadStartIdx = idx;

            // Bottom edge — right to left
            for (int col = gridW - 1; col >= 0; col--)
                nodes.Add(new ConveyorNode
                {
                    index = idx++, localPosition = new Vector2(col + HalfCell, bottom), scanAxis = ScanAxis.Column,
                    alignedGridIndices = BuildRange(col)
                });

            nodes.Add(Corner(idx++, left, bottom));

            // Left edge — bottom to top
            for (int row = 0; row < gridH; row++)
                nodes.Add(new ConveyorNode
                {
                    index = idx++, localPosition = new Vector2(left, row + HalfCell), scanAxis = ScanAxis.Row,
                    alignedGridIndices = BuildRange(row)
                });

            nodes.Add(Corner(idx++, left, top));

            // Top edge — left to right
            for (int col = 0; col < gridW; col++)
                nodes.Add(new ConveyorNode
                {
                    index = idx++, localPosition = new Vector2(col + HalfCell, top), scanAxis = ScanAxis.Column,
                    alignedGridIndices = BuildRange(col)
                });

            nodes.Add(Corner(idx++, right, top));

            // Right edge — top to bottom
            int roadEndIdx = -1;
            for (int row = gridH - 1; row >= 0; row--)
            {
                if (row == 0) roadEndIdx = idx;
                nodes.Add(new ConveyorNode
                {
                    index = idx++, localPosition = new Vector2(right, row + HalfCell), scanAxis = ScanAxis.Row,
                    alignedGridIndices = BuildRange(row)
                });
            }

            nodes.Add(Corner(idx, right, bottom));

            data.nodes = nodes;
            data.roadStartIndex = roadStartIdx;
            data.roadEndIndex = roadEndIdx < 0 ? 0 : roadEndIdx;

            return data;
        }

        public static List<CollectableGridCellData> GetAlignedCells(ConveyorNode node, LevelData level)
        {
            var result = new List<CollectableGridCellData>();

            if (node == null || node.scanAxis == ScanAxis.None)
                return result;

            int w = level.collectableGridWidth;
            int h = level.collectableGridHeight;

            foreach (int lineIdx in node.alignedGridIndices)
            {
                if (node.scanAxis == ScanAxis.Column)
                {
                    if (lineIdx < 0 || lineIdx >= w) continue;
                    bool fromBelow = node.localPosition.y < 0;
                    if (fromBelow)
                        for (int y = 0; y < h; y++)
                            TryAdd(result, level, lineIdx, y);
                    else
                        for (int y = h - 1; y >= 0; y--)
                            TryAdd(result, level, lineIdx, y);
                }
                else
                {
                    if (lineIdx < 0 || lineIdx >= h) continue;
                    bool fromLeft = node.localPosition.x < 0;
                    if (fromLeft)
                        for (int x = 0; x < w; x++)
                            TryAdd(result, level, x, lineIdx);
                    else
                        for (int x = w - 1; x >= 0; x--)
                            TryAdd(result, level, x, lineIdx);
                }
            }

            return result;
        }

        public static Vector2 CanvasCellToLocal(int cx, int cy, int gridH)
        {
            float lx = cx - 1f + HalfCell;
            float ly = (gridH - 1) - (cy - 1f) + HalfCell;
            return new Vector2(lx, ly);
        }

        private static ConveyorNode Corner(int idx, float x, float y) =>
            new ConveyorNode { index = idx, localPosition = new Vector2(x, y), scanAxis = ScanAxis.None };

        private static List<int> BuildRange(int value) => new List<int> { value };

        private static void TryAdd(List<CollectableGridCellData> list, LevelData level, int x, int y)
        {
            var cell = level.GetCollectableCell(x, y);
            if (cell != null && cell.isFilled) list.Add(cell);
        }
    }
}