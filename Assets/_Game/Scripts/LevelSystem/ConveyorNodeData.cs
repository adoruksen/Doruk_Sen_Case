using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.LevelSystem
{
    [Serializable]
    public class ConveyorNodeData
    {
        public int nodeIndex;

        public Vector2Int extendedPosition;

        public ScanAxis scanAxis;

        public bool isCorner;
        public bool isRoadStart;
        public bool isRoadEnd;

        public List<int> alignedGridIndices = new();
    }
}
