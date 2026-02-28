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
            public readonly float BenchRowZ;

            public Result(Vector3 collectablesCenter, Vector3 boxesCenter, Vector3 collectablesBottomLeft, float benchRowZ)
            {
                CollectablesCenter = collectablesCenter;
                BoxesCenter = boxesCenter;
                CollectablesBottomLeft = collectablesBottomLeft;
                BenchRowZ = benchRowZ;
            }
        }

        public static Result Calculate(LevelData data, LevelCreationSettings settings)
        {
            float cellSize = Mathf.Max(0.0001f, settings.CellSize);

            int collectableGridWidth = Mathf.Max(1, data.collectableGridWidth);
            int collectableGridHeight = Mathf.Max(1, data.collectableGridHeight);
            int boxGridWidth = Mathf.Max(1, data.boxGridWidth);
            int boxGridHeight = Mathf.Max(1, data.boxGridHeight);

            float collectOriginX = 0f;
            float collectOriginZ = 0f;

            float benchZ = collectOriginZ - cellSize - settings.ConveyorToBenchGapZ;
            float boxTopZ = benchZ - settings.BenchToBoxGapZ;
            float boxOriginZ = boxTopZ - (boxGridHeight * cellSize);

            float collectCenterX = collectOriginX + (collectableGridWidth * cellSize) * 0.5f;
            float collectCenterZ = collectOriginZ + (collectableGridHeight * cellSize) * 0.5f;

            float boxOriginX = collectCenterX - (boxGridWidth * cellSize) * 0.5f;
            float boxCenterX = boxOriginX + (boxGridWidth * cellSize) * 0.5f;
            float boxCenterZ = boxOriginZ + (boxGridHeight * cellSize) * 0.5f;

            float shiftX = -collectCenterX;
            float shiftZ = -((collectCenterZ + boxCenterZ) * 0.5f);

            var collectablesBottomLeft = new Vector3(collectOriginX + shiftX, 0f, collectOriginZ + shiftZ);
            var collectablesCenter = new Vector3(collectCenterX + shiftX, 0f, collectCenterZ + shiftZ);
            var boxesCenter = new Vector3(boxCenterX + shiftX, 0f, boxCenterZ + shiftZ);
            float benchRowZWorld = benchZ + shiftZ;

            return new Result(collectablesCenter, boxesCenter, collectablesBottomLeft, benchRowZWorld);
        }

        public static Vector3 GridLocalOffsetCentered(Vector2Int cell, int width, int height, float cellSize)
        {
            float x = (cell.x - (width - 1) * 0.5f) * cellSize;
            float z = (cell.y - (height - 1) * 0.5f) * cellSize;
            return new Vector3(x, 0f, z);
        }
    }
}