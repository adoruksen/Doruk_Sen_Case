using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.LevelSystem
{
    [CreateAssetMenu(fileName = "LevelDatabase", menuName = "RubyCase/Level Database", order = -100)]
    public class LevelDatabase : ScriptableObject
    {
        public List<LevelData> levels = new();

        [ShowInInspector, ReadOnly] public int TotalLevels => levels?.Count ?? 0;

        public LevelData GetLevel(int index)
        {
            if (levels == null || index < 0 || index >= levels.Count) return null;
            return levels[index];
        }

        public LevelData GetLevelByID(int id) => levels?.Find(l => l != null && l.levelID == id);

        public int GetIndex(LevelData level) => levels?.IndexOf(level) ?? -1;
    }
}
