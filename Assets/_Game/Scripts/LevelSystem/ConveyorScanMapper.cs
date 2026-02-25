using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.LevelSystem
{
    public static class ConveyorScanMapper
    {
        private const int CellsPerNode = 4;

        public static ConveyorPathData GeneratePath(int gridW, int gridH)
        {
            var data = ScriptableObject.CreateInstance<ConveyorPathData>();
            data.gridWidth  = gridW;
            data.gridHeight = gridH;

            var nodes = new List<ConveyorNodeData>();
            int idx = 0;

            int hNodeCount = Mathf.CeilToInt(gridW / (float)CellsPerNode);
            int vNodeCount = Mathf.CeilToInt(gridH / (float)CellsPerNode);
            
            int roadStartIdx = idx;
            for (int n = hNodeCount - 1; n >= 0; n--)
            {
                int startCol = n * CellsPerNode;
                nodes.Add(BuildColumnNode(idx++, startCol, -2, gridW, isRoadStart: n == hNodeCount - 1));
            }

            nodes.Add(BuildCorner(idx++, new Vector2Int(-2, -2)));

            for (int n = 0; n < vNodeCount; n++)
            {
                int startRow = n * CellsPerNode;
                nodes.Add(BuildRowNode(idx++, -2, startRow, gridH));
            }

            nodes.Add(BuildCorner(idx++, new Vector2Int(-2, gridH + 1)));

            for (int n = 0; n < hNodeCount; n++)
            {
                int startCol = n * CellsPerNode;
                nodes.Add(BuildColumnNode(idx++, startCol, gridH + 1, gridW));
            }

            nodes.Add(BuildCorner(idx++, new Vector2Int(gridW + 1, gridH + 1)));

            int roadEndIdx = idx;
            for (int n = vNodeCount - 1; n >= 0; n--)
            {
                int startRow = n * CellsPerNode;
                nodes.Add(BuildRowNode(idx++, gridW + 1, startRow, gridH, isRoadEnd: n == 0));
                if (n == 0) roadEndIdx = idx - 1;
            }

            nodes.Add(BuildCorner(idx, new Vector2Int(gridW + 1, -2)));

            data.nodes = nodes;
            data.roadStartNodeIndex = roadStartIdx;
            data.roadEndNodeIndex = roadEndIdx;

            return data;
        }

        private static ConveyorNodeData BuildColumnNode(int idx, int startCol, int extY, int gridW, bool isRoadStart = false, bool isRoadEnd = false)
        {
            int endCol  = Mathf.Min(startCol + CellsPerNode - 1, gridW - 1);
            var indices = new List<int>();
            for (int c = startCol; c <= endCol; c++) indices.Add(c);

            return new ConveyorNodeData
            {
                nodeIndex = idx,
                extendedPosition = new Vector2Int(startCol, extY),
                scanAxis = ScanAxis.Column,
                alignedGridIndices = indices,
                isRoadStart = isRoadStart,
                isRoadEnd = isRoadEnd,
            };
        }

        private static ConveyorNodeData BuildRowNode(int idx, int extX, int startRow, int gridH, bool isRoadStart = false, bool isRoadEnd = false)
        {
            int endRow  = Mathf.Min(startRow + CellsPerNode - 1, gridH - 1);
            var indices = new List<int>();
            for (int r = startRow; r <= endRow; r++) indices.Add(r);

            return new ConveyorNodeData
            {
                nodeIndex = idx,
                extendedPosition = new Vector2Int(extX, startRow),
                scanAxis = ScanAxis.Row,
                alignedGridIndices = indices,
                isRoadStart = isRoadStart,
                isRoadEnd = isRoadEnd,
            };
        }

        private static ConveyorNodeData BuildCorner(int idx, Vector2Int extPos)
        {
            return new ConveyorNodeData
            {
                nodeIndex = idx,
                extendedPosition = extPos,
                scanAxis = ScanAxis.None,
                isCorner = true,
            };
        }

        public static List<CollectableGridCellData> GetAlignedCells(ConveyorNodeData node, LevelData level)
        {
            var result = new List<CollectableGridCellData>();
            if (node == null || node.isCorner) return result;

            int w = level.collectableGridWidth;
            int h = level.collectableGridHeight;

            foreach (int lineIdx in node.alignedGridIndices)
            {
                if (node.scanAxis == ScanAxis.Column)
                {
                    if (lineIdx < 0 || lineIdx >= w) continue;

                    bool fromBottom = node.extendedPosition.y < 0;
                    if (fromBottom)
                        for (int y = 0; y < h; y++)
                            AddIfFilled(result, level, lineIdx, y);
                    else
                        for (int y = h - 1; y >= 0; y--)
                            AddIfFilled(result, level, lineIdx, y);
                }
                else
                {
                    if (lineIdx < 0 || lineIdx >= h) continue;

                    bool fromLeft = node.extendedPosition.x < 0;
                    if (fromLeft)
                        for (int x = 0; x < w; x++)
                            AddIfFilled(result, level, x, lineIdx);
                    else
                        for (int x = w - 1; x >= 0; x--)
                            AddIfFilled(result, level, x, lineIdx);
                }
            }

            return result;
        }

        private static void AddIfFilled(List<CollectableGridCellData> list, LevelData level, int x, int y)
        {
            var cell = level.GetCollectableCell(x, y);
            if (cell != null && cell.isFilled) list.Add(cell);
        }

        public static Vector2Int CanvasToExtended(int canvasX, int canvasY, int gridH)
        {
            return new Vector2Int(canvasX - 2, (gridH + 1) - canvasY);
        }

        public static Vector2Int ExtendedToCanvas(Vector2Int ext, int gridH)
        {
            return new Vector2Int(ext.x + 2, (gridH + 1) - ext.y);
        }
    }
}
