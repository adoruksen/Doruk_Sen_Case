using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.LevelSystem
{
    [CreateAssetMenu(fileName = "ConveyorPath_Level_001", menuName = "RubyCase/Conveyor Path Data")]
    public class ConveyorPathData : ScriptableObject
    {
        [ReadOnly] public int gridWidth;
        [ReadOnly] public int gridHeight;

        [ReadOnly] public int roadStartNodeIndex;
        [ReadOnly] public int roadEndNodeIndex;

        [ListDrawerSettings(IsReadOnly = true, ShowIndexLabels = true)]
        public List<ConveyorNodeData> nodes = new();

        public int NodeCount => nodes.Count;

        public ConveyorNodeData GetNode(int index)
        {
            if (index < 0 || index >= nodes.Count) return null;
            return nodes[index];
        }

        public ConveyorNodeData RoadStart => GetNode(roadStartNodeIndex);
        public ConveyorNodeData RoadEnd   => GetNode(roadEndNodeIndex);
    }
}
