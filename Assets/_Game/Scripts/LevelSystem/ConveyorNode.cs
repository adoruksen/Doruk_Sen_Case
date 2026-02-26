using System;
using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.LevelSystem
{
    [Serializable]
    public class ConveyorNode
    {
        public int index;
        public Vector2 localPosition;
        public ScanAxis scanAxis;
        public List<int> alignedGridIndices = new();
        public bool isCorner;
        public bool isRoadStart;
        public bool isRoadEnd;
    }
}
