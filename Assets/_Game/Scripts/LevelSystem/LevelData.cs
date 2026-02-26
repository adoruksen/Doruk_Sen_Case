using System.Collections.Generic;
using System.Linq;
using RubyCase.LevelSystem.Editor;
using UnityEngine;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RubyCase.LevelSystem
{
    [CreateAssetMenu(fileName = "Level_001", menuName = "RubyCase/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Title("Level Settings")]
        public int levelID;
        public int benchCapacity = 4;

        [Title("Collectable Grid")]
        public int collectableGridWidth  = 6;
        public int collectableGridHeight = 6;

        [HideInInspector]
        public List<CollectableGridCellData> collectableCells = new();

        [Title("Box Grid")]
        public int boxGridWidth  = 6;
        public int boxGridHeight = 2;

        [HideInInspector]
        public List<BoxGridCellData> boxCells = new();

        [Title("Conveyor")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        public ConveyorPathData conveyorPath;

        // ---- Grid init ------------------------------------------------------

        public void InitCollectableGrid()
        {
            collectableCells.Clear();
            for (int y = 0; y < collectableGridHeight; y++)
                for (int x = 0; x < collectableGridWidth; x++)
                    collectableCells.Add(new CollectableGridCellData(new Vector2Int(x, y)));
        }

        public void InitBoxGrid()
        {
            boxCells.Clear();
            for (int y = 0; y < boxGridHeight; y++)
                for (int x = 0; x < boxGridWidth; x++)
                    boxCells.Add(new BoxGridCellData(new Vector2Int(x, y)));
        }

        // ---- Accessors ------------------------------------------------------

        public CollectableGridCellData GetCollectableCell(int x, int y) =>
            collectableCells.Find(c => c.position.x == x && c.position.y == y);

        public BoxGridCellData GetBoxCell(int x, int y) =>
            boxCells.Find(c => c.position.x == x && c.position.y == y);

        // ---- Stats ----------------------------------------------------------

        [Title("Stats")]
        [ShowInInspector, ReadOnly] public int TotalCollectables => collectableCells.Count(c => c.isFilled);
        [ShowInInspector, ReadOnly] public int TotalBoxes        => boxCells.Count(c => c.isFilled);
        [ShowInInspector, ReadOnly] public int ConveyorNodes     => conveyorPath?.NodeCount ?? 0;

        // ---- Validate -------------------------------------------------------

        [Button("Validate Level"), PropertyOrder(100)]
        private void ValidateLevel()
        {
#if UNITY_EDITOR
            var result = LevelValidator.Validate(this);
            EditorUtility.DisplayDialog(
                result.IsValid ? "Valid" : "Invalid",
                result.IsValid
                    ? $"Level is valid. ({result.Warnings.Count} warnings)"
                    : $"{result.Errors.Count} error(s):\n\n" + string.Join("\n", result.Errors),
                "OK");
#endif
        }
    }
}
