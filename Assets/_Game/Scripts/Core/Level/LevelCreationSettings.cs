using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.Core
{
    [CreateAssetMenu(fileName = "LevelCreationSettings", menuName = "RubyCase/Level Creation Settings")]
    public class LevelCreationSettings : ScriptableObject
    {
        [Title("Grid")]
        public float CellSize = 1f;

        [Title("Layout")]
        [Min(1f)] public float ConveyorToBenchGapZ = 1f;
        [Min(1f)] public float BenchToBoxGapZ      = 1f;
        [Min(0.5f)] public float BenchSpacingX    = 1.25f;
    }
}
