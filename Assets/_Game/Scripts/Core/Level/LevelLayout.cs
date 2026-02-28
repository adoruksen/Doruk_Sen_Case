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
            int gridCount = Mathf.Max(1, data.collectableGridWidth); // square grid

            float cellSize = s.GetCellSize(gridCount);
            float cellPitch = s.GetCellPitch(gridCount);
            float bp = Mathf.Max(0.0001f, s.BoxPitch);

            int bw = Mathf.Max(1, data.boxGridWidth);
            int bh = Mathf.Max(1, data.boxGridHeight);

            // Collectable grid world size is always ConveyorInnerSize x ConveyorInnerSize
            float innerSize = s.ConveyorInnerSize;

            float collectCenterX = innerSize * 0.5f;
            float collectCenterZ = innerSize * 0.5f;

            // Bench sits below the outer face of the conveyor ring
            float conveyorOuterEdge = s.GetConveyorOuterEdge(gridCount);
            float benchZ = -(conveyorOuterEdge + s.ConveyorToBenchGap);
            float boxTopZ = benchZ - s.BenchToBoxGap;
            float boxOriginZ = boxTopZ - bh * bp;

            float boxOriginX = collectCenterX - bw * bp * 0.5f;
            float boxCenterX = boxOriginX + bw * bp * 0.5f;
            float boxCenterZ = boxOriginZ + bh * bp * 0.5f;

            float shiftX = -collectCenterX;
            float shiftZ = -((collectCenterZ + boxCenterZ) * 0.5f);

            var collectBL = new Vector3(shiftX, 0f, shiftZ);
            var collectCenter = new Vector3(collectCenterX + shiftX, 0f, collectCenterZ + shiftZ);
            var conveyorCenter = collectCenter; // conveyor is centered on the collectable grid
            var boxCenter = new Vector3(boxCenterX + shiftX, 0f, boxCenterZ + shiftZ);

            return new Result(collectCenter, boxCenter, collectBL, conveyorCenter,
                cellSize, cellPitch, benchZ + shiftZ);
        }

        public static Vector3 GridLocalOffsetCentered(Vector2Int cell, int width, int height, float pitch)
        {
            float x = (cell.x - (width - 1) * 0.5f) * pitch;
            float z = (cell.y - (height - 1) * 0.5f) * pitch;
            return new Vector3(x, 0f, z);
        }
    }
}