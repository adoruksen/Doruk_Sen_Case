using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.LevelSystem
{
    [Serializable]
    public class ConveyorNode
    {
        // Position in the ordered path (0-based).
        public int index;

        // Center position of this node in grid-unit space, relative to grid origin (0,0).
        // X maps to the horizontal axis, Y maps to the depth/vertical axis.
        // Runtime: worldPosition = gridOrigin + new Vector3(localPosition.x, 0, localPosition.y) * cellSize
        public Vector2 localPosition;

        // Which axis of the collectable grid this node faces.
        public ScanAxis scanAxis;

        // Grid column indices (if scanAxis == Column) or row indices (if scanAxis == Row)
        // that are directly aligned with this node.
        // Always 0 entries for corner nodes, 1-4 entries for edge nodes.
        public List<int> alignedGridIndices = new();

        // Flags
        public bool isCorner;
        public bool isRoadStart;
        public bool isRoadEnd;
    }
}
