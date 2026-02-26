using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.LevelSystem
{
    [CreateAssetMenu(fileName = "ConveyorPath_Level_001", menuName = "RubyCase/Conveyor Path Data")]
    public class ConveyorPathData : ScriptableObject
    {
        [BoxGroup("Grid Info"), ReadOnly] public int gridWidth;
        [BoxGroup("Grid Info"), ReadOnly] public int gridHeight;

        // Physical size of one grid cell in world units.
        // Set this to match your scene's grid cell size.
        // Used at runtime to convert localPosition → worldPosition.
        [BoxGroup("Grid Info")] public float cellSize = 1f;

        [BoxGroup("Path Info"), ReadOnly] public int roadStartIndex;
        [BoxGroup("Path Info"), ReadOnly] public int roadEndIndex;

        [ListDrawerSettings(IsReadOnly = true, ShowIndexLabels = true)]
        public List<ConveyorNode> nodes = new();

        // ---- Accessors -------------------------------------------------------

        public int NodeCount => nodes.Count;

        public ConveyorNode GetNode(int index)
        {
            if (index < 0 || index >= nodes.Count) return null;
            return nodes[index];
        }

        public ConveyorNode RoadStart => GetNode(roadStartIndex);
        public ConveyorNode RoadEnd   => GetNode(roadEndIndex);

        public Vector3 GetWorldPosition(int nodeIndex, Vector3 gridOrigin)
        {
            var node = GetNode(nodeIndex);
            if (node == null) return gridOrigin;
            return gridOrigin + new Vector3(node.localPosition.x, 0f, node.localPosition.y) * cellSize;
        }

        public Vector3[] GetWorldPath(Vector3 gridOrigin)
        {
            var path = new Vector3[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
                path[i] = GetWorldPosition(i, gridOrigin);
            return path;
        }
    }
}
