using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.LevelSystem
{
    public static class ConveyorScanMapper
    {
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

            for (int col = gridW - 1; col >= 0; col--)
                nodes.Add(new ConveyorNode
                {
                    index = idx++, localPosition = new Vector2(col + HalfCell, bottom), scanAxis = ScanAxis.Column,
                    alignedGridIndices = BuildRange(col)
                });

            nodes.Add(Corner(idx++, left, bottom));

            for (int row = 0; row < gridH; row++)
                nodes.Add(new ConveyorNode
                {
                    index = idx++, localPosition = new Vector2(left, row + HalfCell), scanAxis = ScanAxis.Row,
                    alignedGridIndices = BuildRange(row)
                });

            nodes.Add(Corner(idx++, left, top));

            for (int col = 0; col < gridW; col++)
                nodes.Add(new ConveyorNode
                {
                    index = idx++, localPosition = new Vector2(col + HalfCell, top), scanAxis = ScanAxis.Column,
                    alignedGridIndices = BuildRange(col)
                });

            nodes.Add(Corner(idx++, right, top));

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

        // Returns the nearest filled cell to the conveyor side, within half the grid depth.
        // Returns null if nothing is in range or all cells are empty/already collected.
        public static CollectableGridCellData GetNearestCell(ConveyorNode node, LevelData level)
        {
            if (node == null || node.scanAxis == ScanAxis.None) return null;

            int w = level.collectableGridWidth;
            int h = level.collectableGridHeight;

            foreach (int lineIdx in node.alignedGridIndices)
            {
                if (node.scanAxis == ScanAxis.Column)
                {
                    if (lineIdx < 0 || lineIdx >= w) continue;

                    bool fromBelow = node.localPosition.y < 0;
                    int maxDepth = Mathf.Max(1, h / 2);

                    if (fromBelow)
                    {
                        for (int y = 0; y < maxDepth; y++)
                        {
                            var cell = level.GetCollectableCell(lineIdx, y);
                            if (IsCollectable(cell)) return cell;
                        }
                    }
                    else
                    {
                        for (int y = h - 1; y >= h - maxDepth; y--)
                        {
                            var cell = level.GetCollectableCell(lineIdx, y);
                            if (IsCollectable(cell)) return cell;
                        }
                    }
                }
                else
                {
                    if (lineIdx < 0 || lineIdx >= h) continue;

                    bool fromLeft = node.localPosition.x < 0;
                    int maxDepth = Mathf.Max(1, w / 2);

                    if (fromLeft)
                    {
                        for (int x = 0; x < maxDepth; x++)
                        {
                            var cell = level.GetCollectableCell(x, lineIdx);
                            if (IsCollectable(cell)) return cell;
                        }
                    }
                    else
                    {
                        for (int x = w - 1; x >= w - maxDepth; x--)
                        {
                            var cell = level.GetCollectableCell(x, lineIdx);
                            if (IsCollectable(cell)) return cell;
                        }
                    }
                }
            }

            return null;
        }

        public static Vector2 CanvasCellToLocal(int cx, int cy, int gridH)
        {
            float lx = cx - 1f + HalfCell;
            float ly = (gridH - 1) - (cy - 1f) + HalfCell;
            return new Vector2(lx, ly);
        }

        private static bool IsCollectable(CollectableGridCellData cell) =>
            cell != null && cell.isFilled && cell.SpawnedObject != null;

        private static ConveyorNode Corner(int idx, float x, float y) =>
            new ConveyorNode { index = idx, localPosition = new Vector2(x, y), scanAxis = ScanAxis.None };

        private static List<int> BuildRange(int value) => new List<int> { value };
    }
}