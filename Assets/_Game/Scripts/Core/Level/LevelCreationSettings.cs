using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.Core
{
    [CreateAssetMenu(fileName = "LevelCreationSettings", menuName = "RubyCase/Level Creation Settings")]
    public class LevelCreationSettings : ScriptableObject
    {
        [Title("Conveyor")]
        [Tooltip("Fixed world-space size of the conveyor inner area. Grid always fills this exactly.")]
        public float ConveyorInnerSize = 8f;

        [Tooltip("World-space gap between the grid edge and the inner face of the conveyor ring.")] [Min(0f)]
        public float ConveyorGridGap = 0.25f;

        [Title("Collectable Spacing")]
        [Tooltip("Gap between items as a fraction of cell size. 0.1 = 10% of cell size.")]
        [Range(0f, 0.5f)]
        public float CollectableSpacingRatio = 0.08f;

        [Title("Box Grid")] public float BoxCellSize = 1f;
        [Min(0f)] public float BoxSpacing = 0.1f;

        [Title("Layout")] [Min(0f)] public float ConveyorToBenchGap = 0.5f;
        [Min(0f)] public float BenchToBoxGap = 1f;
        [Min(0.5f)] public float BenchSpacingX = 1.25f;

        // Called with the current grid size to get runtime cell size.
        public float GetCellSize(int gridCount) => ConveyorInnerSize / Mathf.Max(1, gridCount);
        public float GetCellPitch(int gridCount) => GetCellSize(gridCount) * (1f + CollectableSpacingRatio);
        public float BoxPitch => BoxCellSize + BoxSpacing;

        // Outer edge of conveyor ring from grid origin = half a cell + gap
        public float GetConveyorOuterEdge(int gridCount) => GetCellSize(gridCount) * 0.5f + ConveyorGridGap;
    }
}