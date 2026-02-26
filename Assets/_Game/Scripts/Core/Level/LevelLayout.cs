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
            public readonly Vector3 BoxesBottomLeft;
            public readonly float BenchRowZ;

            public Result(Vector3 collectablesCenter, Vector3 boxesCenter, Vector3 collectablesBottomLeft,
                Vector3 boxesBottomLeft, float benchRowZ)
            {
                CollectablesCenter = collectablesCenter;
                BoxesCenter = boxesCenter;
                CollectablesBottomLeft = collectablesBottomLeft;
                BoxesBottomLeft = boxesBottomLeft;
                BenchRowZ = benchRowZ;
            }
        }

        public static Result Calculate(LevelData data, LevelCreationSettings settings)
        {
            float cellSize = Mathf.Max(0.0001f, settings.CellSize);

            int collectableGridWidth = Mathf.Max(1, data.collectableGridWidth);
            int collectibleGridHeight = Mathf.Max(1, data.collectableGridHeight);
            int boxGridWitdh = Mathf.Max(1, data.boxGridWidth);
            int boxGridHeight = Mathf.Max(1, data.boxGridHeight);

            float collectOriginX = 0f;
            float collectOriginZ = 0f;

            float conveyorBottomZ = collectOriginZ - cellSize;
            float benchZ = conveyorBottomZ - settings.ConveyorToBenchGapZ;
            float boxTopZ = benchZ - settings.BenchToBoxGapZ;
            float boxOriginZ = boxTopZ - (boxGridHeight * cellSize);

            float collectCenterX = collectOriginX + (collectableGridWidth * cellSize) * 0.5f;
            float collectCenterZ = collectOriginZ + (collectibleGridHeight * cellSize) * 0.5f;

            float boxOriginX = collectCenterX - (boxGridWitdh * cellSize) * 0.5f;
            float boxCenterX = boxOriginX + (boxGridWitdh * cellSize) * 0.5f;
            float boxCenterZ = boxOriginZ + (boxGridHeight * cellSize) * 0.5f;

            float midX = collectCenterX;
            float midZ = (collectCenterZ + boxCenterZ) * 0.5f;

            float shiftX = -midX;
            float shiftZ = -midZ;

            var collectablesBottomLeft = new Vector3(collectOriginX + shiftX, 0f, collectOriginZ + shiftZ);
            var boxesBottomLeft = new Vector3(boxOriginX + shiftX, 0f, boxOriginZ + shiftZ);

            var collectablesCenter = new Vector3(collectCenterX + shiftX, 0f, collectCenterZ + shiftZ);
            var boxesCenter = new Vector3(boxCenterX + shiftX, 0f, boxCenterZ + shiftZ);
            float benchRowZWorld = benchZ + shiftZ;

            return new Result(collectablesCenter, boxesCenter, collectablesBottomLeft, boxesBottomLeft, benchRowZWorld);
        }

        public static Vector3 GridLocalOffsetCentered(Vector2Int cell, int width, int height, float cellSize)
        {
            float x = (cell.x - (width - 1) * 0.5f) * cellSize;
            float z = (cell.y - (height - 1) * 0.5f) * cellSize;
            return new Vector3(x, 0f, z);
        }
    }
}
