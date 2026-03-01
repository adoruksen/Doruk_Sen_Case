using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.Core
{
    [CreateAssetMenu(fileName = "LevelCreationSettings", menuName = "RubyCase/Level Creation Settings")]
    public class LevelCreationSettings : ScriptableObject
    {
        [Title("Conveyor")]
        public float ConveyorInnerSize = 8f;

        [Min(0f)]
        public float ConveyorGridGap = 0.25f;

        public float ConveyorSpeed = 4f;
        
        public float WaypointInset = 0f;

        public float WaypointY = 0f;

        [Range(0f, 0.5f)]
        public float CollectableSpacingRatio = 0.08f;

        [Title("Box Grid")] public float BoxCellSize = 1f;
        [Min(0f)] public float BoxSpacing = 0.1f;

        public Vector3 CollectablesCenter = new(0f, 0f, 2f);

        public Vector3 BoxesCenter = new(0f, 0f, -4f);

        public float BenchRowZ = -2f;
        
        public float BenchSpacingX = 1.5f;

        
        public float GetCellSize(int gridCount) => ConveyorInnerSize / Mathf.Max(1, gridCount);
        public float GetCellPitch(int gridCount) => GetCellSize(gridCount) * (1f + CollectableSpacingRatio);
        public float BoxPitch => BoxCellSize + BoxSpacing;
        public float GetConveyorOuterEdge(int gridCount) => GetCellSize(gridCount) * 0.5f + ConveyorGridGap;

        public Vector3 GetCollectablesBottomLeft(int gridCount)
        {
            float halfSize = ConveyorInnerSize * 0.5f;
            return new Vector3(
                CollectablesCenter.x - halfSize,
                CollectablesCenter.y,
                CollectablesCenter.z - halfSize);
        }
    }
}