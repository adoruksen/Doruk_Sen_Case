using System.Collections.Generic;
using RubyCase.TeamSystem;

namespace RubyCase.LevelSystem.Editor
{
    public static class LevelValidator
    {
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
            CheckConveyorPath(level, r);
            CheckColorBalance(level, r);
            CheckBench(level, r);

            return r;
        }

        private static void CheckCollectableGrid(LevelData level, Result r)
        {
            if (level.collectableCells == null || level.collectableCells.Count == 0)
                r.AddError("Collectable grid not initialized.");
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

        private static void CheckConveyorPath(LevelData level, Result r)
        {
            if (level.conveyorPath == null)
            {
                r.AddError("ConveyorPathData not generated. Re-init the collectable grid.");
                return;
            }

            if (level.conveyorPath.NodeCount < 4)
                r.AddError("Conveyor path has fewer than 4 nodes.");
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

            var boxCapacity = new Dictionary<Team, int>();
            foreach (var c in level.boxCells)
            {
                if (!c.isFilled || c.team == null) continue;
                boxCapacity.TryAdd(c.team, 0);
                boxCapacity[c.team] += c.capacityOverride > 0 ? c.capacityOverride : 3;
            }

            foreach (var kv in collectableCount)
            {
                boxCapacity.TryGetValue(kv.Key, out int cap);
                if (cap == 0)
                    r.AddError($"Team '{kv.Key.name}': {kv.Value} collectables but no box.");
                else if (cap < kv.Value)
                    r.AddError($"Team '{kv.Key.name}': boxes hold {cap}, need {kv.Value}. Unsolvable.");
                else if (cap > kv.Value)
                    r.AddWarning($"Team '{kv.Key.name}': excess capacity {cap - kv.Value}.");
            }

            foreach (var kv in boxCapacity)
                if (!collectableCount.ContainsKey(kv.Key))
                    r.AddWarning($"Team '{kv.Key.name}': box placed but no matching collectables.");
        }

        private static void CheckBench(LevelData level, Result r)
        {
            if (level.benchCapacity < 1)
                r.AddError("Bench capacity must be at least 1.");
        }
    }
}
