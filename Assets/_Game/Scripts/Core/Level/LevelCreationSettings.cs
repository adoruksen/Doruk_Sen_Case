using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.Core
{
    [CreateAssetMenu(fileName = "LevelCreationSettings", menuName = "RubyCase/Level Creation Settings")]
    public class LevelCreationSettings : ScriptableObject
    {
        public float ConveyorInnerSize = 8f;

        public float ConveyorGridGap = 0.25f;

        public float ConveyorSpeed = 4f;

        public float WaypointInset = .5f;

        public float WaypointY = 1f;

        [Range(0f, 0.5f)]
        public float CollectableSpacingRatio = 0.08f;

        [Title("Box Grid")] public float BoxCellSize = 1f;
        [Min(0f)] public float BoxSpacing = 0.1f;

        [Title("Layout")] [Min(0f)] public float ConveyorToBenchGap = 0.5f;
        [Min(0f)] public float BenchToBoxGap = 1f;
        [Min(0.5f)] public float BenchSpacingX = 1.25f;

        public float GetCellSize(int gridCount) => ConveyorInnerSize / Mathf.Max(1, gridCount);
        public float GetCellPitch(int gridCount) => GetCellSize(gridCount) * (1f + CollectableSpacingRatio);
        public float BoxPitch => BoxCellSize + BoxSpacing;
        public float GetConveyorOuterEdge(int gridCount) => GetCellSize(gridCount) * 0.5f + ConveyorGridGap;
    }
}