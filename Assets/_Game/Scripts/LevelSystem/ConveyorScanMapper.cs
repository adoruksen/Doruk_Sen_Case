using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.LevelSystem
{
    public static class ConveyorScanMapper
    {
        // Each conveyor node spans this many grid cells.
        public const int CellsPerNode = 4;

        // Offset in grid units between the grid edge and the conveyor node center.
        // The conveyor sits 2 units outside the grid.
        public const float ConveyorOffset = 2f;

        // ---- Path Generation ------------------------------------------------
        //
        // Direction (matching spec):
        //   Bottom edge : right  → left   (boxes enter here, Road Start = rightmost)
        //   Left edge   : bottom → top
        //   Top edge    : left   → right
        //   Right edge  : top    → bottom (boxes exit here, Road End = bottommost)
        //
        // Node local positions are the center of each node's 4-cell span, expressed
        // in grid units relative to grid origin (0,0) bottom-left.
        //
        // Conveyor center positions:
        //   Bottom : y = -ConveyorOffset                     (below grid)
        //   Left   : x = -ConveyorOffset                     (left of grid)
        //   Top    : y =  gridHeight - 1 + ConveyorOffset    (above grid)
        //   Right  : x =  gridWidth  - 1 + ConveyorOffset    (right of grid)

        public static ConveyorPathData GeneratePath(int gridW, int gridH)
        {
            var data   = ScriptableObject.CreateInstance<ConveyorPathData>();
            data.gridWidth  = gridW;
            data.gridHeight = gridH;

            var nodes = new List<ConveyorNode>();
            int idx   = 0;

            int hNodeCount = Mathf.CeilToInt(gridW / (float)CellsPerNode);
            int vNodeCount = Mathf.CeilToInt(gridH / (float)CellsPerNode);

            float bottomY = -ConveyorOffset;
            float topY    = (gridH - 1) + ConveyorOffset;
            float leftX   = -ConveyorOffset;
            float rightX  = (gridW - 1) + ConveyorOffset;

            // ---- Bottom edge: right → left ---------------------------------
            int roadStartIdx = idx;
            for (int n = hNodeCount - 1; n >= 0; n--)
            {
                int startCol = n * CellsPerNode;
                int endCol   = Mathf.Min(startCol + CellsPerNode - 1, gridW - 1);
                float centerX = (startCol + endCol) * 0.5f;

                nodes.Add(new ConveyorNode
                {
                    index               = idx,
                    localPosition       = new Vector2(centerX, bottomY),
                    scanAxis            = ScanAxis.Column,
                    alignedGridIndices  = BuildRange(startCol, endCol),
                    isRoadStart         = n == hNodeCount - 1,
                });
                idx++;
            }

            // ---- Bottom-left corner ----------------------------------------
            nodes.Add(new ConveyorNode
            {
                index         = idx++,
                localPosition = new Vector2(leftX, bottomY),
                scanAxis      = ScanAxis.None,
                isCorner      = true,
            });

            // ---- Left edge: bottom → top ------------------------------------
            for (int n = 0; n < vNodeCount; n++)
            {
                int startRow = n * CellsPerNode;
                int endRow   = Mathf.Min(startRow + CellsPerNode - 1, gridH - 1);
                float centerY = (startRow + endRow) * 0.5f;

                nodes.Add(new ConveyorNode
                {
                    index              = idx++,
                    localPosition      = new Vector2(leftX, centerY),
                    scanAxis           = ScanAxis.Row,
                    alignedGridIndices = BuildRange(startRow, endRow),
                });
            }

            // ---- Top-left corner -------------------------------------------
            nodes.Add(new ConveyorNode
            {
                index         = idx++,
                localPosition = new Vector2(leftX, topY),
                scanAxis      = ScanAxis.None,
                isCorner      = true,
            });

            // ---- Top edge: left → right -------------------------------------
            for (int n = 0; n < hNodeCount; n++)
            {
                int startCol = n * CellsPerNode;
                int endCol   = Mathf.Min(startCol + CellsPerNode - 1, gridW - 1);
                float centerX = (startCol + endCol) * 0.5f;

                nodes.Add(new ConveyorNode
                {
                    index              = idx++,
                    localPosition      = new Vector2(centerX, topY),
                    scanAxis           = ScanAxis.Column,
                    alignedGridIndices = BuildRange(startCol, endCol),
                });
            }

            // ---- Top-right corner ------------------------------------------
            nodes.Add(new ConveyorNode
            {
                index         = idx++,
                localPosition = new Vector2(rightX, topY),
                scanAxis      = ScanAxis.None,
                isCorner      = true,
            });

            // ---- Right edge: top → bottom -----------------------------------
            int roadEndIdx = idx + vNodeCount - 1;
            for (int n = vNodeCount - 1; n >= 0; n--)
            {
                int startRow = n * CellsPerNode;
                int endRow   = Mathf.Min(startRow + CellsPerNode - 1, gridH - 1);
                float centerY = (startRow + endRow) * 0.5f;
                bool isEnd   = n == 0;

                nodes.Add(new ConveyorNode
                {
                    index              = idx++,
                    localPosition      = new Vector2(rightX, centerY),
                    scanAxis           = ScanAxis.Row,
                    alignedGridIndices = BuildRange(startRow, endRow),
                    isRoadEnd          = isEnd,
                });
            }

            // ---- Bottom-right corner (closes the loop) ---------------------
            nodes.Add(new ConveyorNode
            {
                index         = idx,
                localPosition = new Vector2(rightX, bottomY),
                scanAxis      = ScanAxis.None,
                isCorner      = true,
            });

            data.nodes          = nodes;
            data.roadStartIndex = roadStartIdx;
            data.roadEndIndex   = roadEndIdx;

            return data;
        }

        // ---- Runtime: get aligned collectable cells -------------------------
        //
        // Call this each time a box arrives at a new node.
        // Returns filled collectable cells in the aligned columns/rows,
        // ordered nearest-to-conveyor first so the box grabs the closest item.

        public static List<CollectableGridCellData> GetAlignedCells(
            ConveyorNode node,
            LevelData    level)
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
                    if (fromBelow) for (int y = 0;     y < h;  y++) TryAdd(result, level, lineIdx, y);
                    else           for (int y = h - 1; y >= 0; y--) TryAdd(result, level, lineIdx, y);
                }
                else
                {
                    if (lineIdx < 0 || lineIdx >= h) continue;
                    bool fromLeft = node.localPosition.x < 0;
                    if (fromLeft) for (int x = 0;     x < w;  x++) TryAdd(result, level, x, lineIdx);
                    else          for (int x = w - 1; x >= 0; x--) TryAdd(result, level, x, lineIdx);
                }
            }

            return result;
        }

        // ---- Editor helpers -------------------------------------------------

        // Convert canvas pixel cell (cx, cy) to localPosition for matching nodes in the path.
        // Canvas has 2 border cells on each side (gap + conveyor).
        // Returns the center local position of that canvas cell.
        public static Vector2 CanvasCellToLocal(int cx, int cy, int gridH)
        {
            float lx = cx - 2f + 0.5f;
            float ly = (gridH - 1) - (cy - 2f) + 0.5f;
            return new Vector2(lx, ly);
        }

        // ---- Helpers --------------------------------------------------------

        private static List<int> BuildRange(int from, int to)
        {
            var list = new List<int>();
            for (int i = from; i <= to; i++) list.Add(i);
            return list;
        }

        private static void TryAdd(
            List<CollectableGridCellData> list,
            LevelData level, int x, int y)
        {
            var cell = level.GetCollectableCell(x, y);
            if (cell != null && cell.isFilled) list.Add(cell);
        }
    }
}
