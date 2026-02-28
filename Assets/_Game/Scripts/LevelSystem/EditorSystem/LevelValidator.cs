using System.Collections.Generic;
using RubyCase.TeamSystem;

namespace RubyCase.LevelSystem.Editor
{
    public static class LevelValidator
    {
        private const int BoxSlotCapacity = 4;

        public sealed class Result
        {
            public bool IsValid = true;
            public readonly List<string> Errors = new();
            public readonly List<string> Warnings = new();

            internal void AddError(string msg)
            {
                IsValid = false;
                Errors.Add(msg);
            }

            internal void AddWarning(string msg)
            {
                Warnings.Add(msg);
            }
        }

        public static Result Validate(LevelData level)
        {
            var r = new Result();
            if (level == null)
            {
                r.AddError("LevelData is null.");
                return r;
            }

            CheckCollectableGrid(level, r);
            CheckBoxGrid(level, r);
            CheckColorBalance(level, r);
            CheckBench(level, r);

            return r;
        }

        private static void CheckCollectableGrid(LevelData level, Result r)
        {
            if (level.collectableCells == null || level.collectableCells.Count == 0)
                r.AddError("Collectable grid not initialized.");

            if (level.collectableGridWidth != level.collectableGridHeight)
                r.AddWarning(
                    $"Grid is not square ({level.collectableGridWidth}x{level.collectableGridHeight}). Conveyor assumes square grid.");
        }

        private static void CheckBoxGrid(LevelData level, Result r)
        {
            if (level.boxCells == null || level.boxCells.Count == 0)
            {
                r.AddError("Box grid not initialized.");
                return;
            }

            bool any = false;
            foreach (var c in level.boxCells)
                if (c.isFilled)
                {
                    any = true;
                    break;
                }

            if (!any) r.AddError("Box grid has no boxes placed.");
        }
        
        private static void CheckColorBalance(LevelData level, Result r)
        {
            if (level.collectableCells == null || level.boxCells == null) return;

            var collectableCount = new Dictionary<Team, int>();
            foreach (var c in level.collectableCells)
            {
                if (!c.isFilled || c.team == null) continue;
                collectableCount.TryAdd(c.team, 0);
                collectableCount[c.team]++;
            }

            var boxCount = new Dictionary<Team, int>();
            foreach (var c in level.boxCells)
            {
                if (!c.isFilled || c.team == null) continue;
                boxCount.TryAdd(c.team, 0);
                boxCount[c.team]++;
            }

            foreach (var kv in collectableCount)
            {
                var team = kv.Key;
                int collectables = kv.Value;

                boxCount.TryGetValue(team, out int boxes);

                if (boxes == 0)
                {
                    r.AddError($"Team '{team.name}': {collectables} collectables but no box.");
                    continue;
                }

                int needed = boxes * BoxSlotCapacity;

                if (collectables < needed)
                    r.AddWarning(
                        $"Team '{team.name}': {boxes} box(es) hold {needed} slots but only {collectables} collectables. {needed - collectables} slot(s) will be empty.");
                else if (collectables > needed)
                    r.AddError(
                        $"Team '{team.name}': {collectables} collectables but {boxes} box(es) only hold {needed}. Level unsolvable — add {(collectables - needed + BoxSlotCapacity - 1) / BoxSlotCapacity} more box(es) or remove {collectables - needed} collectable(s).");
            }

            foreach (var kv in boxCount)
                if (!collectableCount.ContainsKey(kv.Key))
                    r.AddWarning($"Team '{kv.Key.name}': {kv.Value} box(es) placed but no collectables.");
        }

        private static void CheckBench(LevelData level, Result r)
        {
            if (level.benchCapacity < 1)
                r.AddError("Bench capacity must be at least 1.");
        }
    }
}