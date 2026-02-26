using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.LevelSystem
{
    public static class ConveyorScanMapper
    {
        public const int CellsPerNode = 1;

        public static ConveyorPathData GeneratePath(int gridW, int gridH)
        {
            var data = ScriptableObject.CreateInstance<ConveyorPathData>();
            data.gridWidth = gridW;
            data.gridHeight = gridH;

            var nodes = new List<ConveyorNode>();

            int idx = 0;
            float leftX = -0.5f;
            float rightX = gridW + 0.5f;
            float bottomY = -0.5f;
            float topY = gridH + 0.5f;

            int roadStartIdx = idx;
            for (int col = gridW - 1; col >= 0; col--)
            {
                nodes.Add(new ConveyorNode
                {
                    index = idx++,
                    localPosition = new Vector2(col + 0.5f, bottomY),
                    scanAxis = ScanAxis.Column,
                    alignedGridIndices = BuildRange(col, col),
                    isRoadStart = col == gridW - 1,
                });
            }

            nodes.Add(new ConveyorNode
            {
                index = idx++,
                localPosition = new Vector2(leftX, bottomY),
                scanAxis = ScanAxis.None,
                isCorner = true,
            });

            for (int row = 0; row < gridH; row++)
            {
                nodes.Add(new ConveyorNode
                {
                    index = idx++,
                    localPosition = new Vector2(leftX, row + 0.5f),
                    scanAxis = ScanAxis.Row,
                    alignedGridIndices = BuildRange(row, row),
                });
            }

            nodes.Add(new ConveyorNode
            {
                index = idx++,
                localPosition = new Vector2(leftX, topY),
                scanAxis = ScanAxis.None,
                isCorner = true,
            });

            for (int col = 0; col < gridW; col++)
            {
                nodes.Add(new ConveyorNode
                {
                    index = idx++,
                    localPosition = new Vector2(col + 0.5f, topY),
                    scanAxis = ScanAxis.Column,
                    alignedGridIndices = BuildRange(col, col),
                });
            }

            nodes.Add(new ConveyorNode
            {
                index = idx++,
                localPosition = new Vector2(rightX, topY),
                scanAxis = ScanAxis.None,
                isCorner = true,
            });

            int roadEndIdx = -1;
            for (int row = gridH - 1; row >= 0; row--)
            {
                bool isEnd = row == 0;
                if (isEnd) roadEndIdx = idx;

                nodes.Add(new ConveyorNode
                {
                    index = idx++,
                    localPosition = new Vector2(rightX, row + 0.5f),
                    scanAxis = ScanAxis.Row,
                    alignedGridIndices = BuildRange(row, row),
                    isRoadEnd = isEnd,
                });
            }

            nodes.Add(new ConveyorNode
            {
                index = idx,
                localPosition = new Vector2(rightX, bottomY),
                scanAxis = ScanAxis.None,
                isCorner = true,
            });

            data.nodes = nodes;
            data.roadStartIndex = roadStartIdx;
            data.roadEndIndex = roadEndIdx < 0 ? 0 : roadEndIdx;

            return data;
        }


        public static List<CollectableGridCellData> GetAlignedCells(ConveyorNode node, LevelData level)
        {
            var result = new List<CollectableGridCellData>();

            if (node == null || node.isCorner || node.scanAxis == ScanAxis.None)
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
            float lx = cx - 1f + 0.5f;
            float ly = (gridH - 1) - (cy - 1f) + 0.5f;
            return new Vector2(lx, ly);
        }


        private static List<int> BuildRange(int from, int to)
        {
            var list = new List<int>();
            for (int i = from; i <= to; i++) list.Add(i);
            return list;
        }

        private static void TryAdd(List<CollectableGridCellData> list, LevelData level, int x, int y)
        {
            var cell = level.GetCollectableCell(x, y);
            if (cell != null && cell.isFilled) list.Add(cell);
        }
    }
}
