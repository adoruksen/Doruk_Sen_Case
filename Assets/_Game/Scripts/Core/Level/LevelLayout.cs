using UnityEngine;
using RubyCase.LevelSystem;

namespace RubyCase.Core
{
    public static class LevelLayout
    {
        public readonly struct Result
        {
            public readonly Vector3 CollectablesCenter;
            public readonly Vector3 BoxesCenter;
            public readonly Vector3 CollectablesBottomLeft;
            public readonly Vector3 ConveyorCenter;
            public readonly float CellSize;
            public readonly float CellPitch;
            public readonly float BenchRowZ;

            public Result(Vector3 collectablesCenter, Vector3 boxesCenter,
                Vector3 collectablesBottomLeft, Vector3 conveyorCenter,
                float cellSize, float cellPitch, float benchRowZ)
            {
                CollectablesCenter = collectablesCenter;
                BoxesCenter = boxesCenter;
                CollectablesBottomLeft = collectablesBottomLeft;
                ConveyorCenter = conveyorCenter;
                CellSize = cellSize;
                CellPitch = cellPitch;
                BenchRowZ = benchRowZ;
            }
        }
        
        public static Result Calculate(LevelData data, LevelCreationSettings s)
        {
            int gridCount = Mathf.Max(1, data.collectableGridWidth);

            return new Result(
                collectablesCenter: s.CollectablesCenter,
                boxesCenter: s.BoxesCenter,
                collectablesBottomLeft: s.GetCollectablesBottomLeft(gridCount),
                conveyorCenter: s.CollectablesCenter,
                cellSize: s.GetCellSize(gridCount),
                cellPitch: s.GetCellPitch(gridCount),
                benchRowZ: s.BenchRowZ);
        }

        public static Vector3 GridLocalOffsetCentered(Vector2Int cell, int width, int height, float pitch)
        {
            float x = (cell.x - (width - 1) * 0.5f) * pitch;
            float z = (cell.y - (height - 1) * 0.5f) * pitch;
            return new Vector3(x, 0f, z);
        }
    }
}