using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.TeamSystem
{
    /// <summary>
    /// Single asset that lists all Team assets in the project.
    /// Level Editor reads teams from here — no folder scanning needed.
    /// Create one at: Assets/_Game/Scriptables/TeamDatabase.asset
    /// </summary>
    [CreateAssetMenu(menuName = "Game/TeamSystem/TeamDatabase", order = -398)]
    public class TeamDatabase : ScriptableObject
    {
        [Title("Teams")]
        [InfoBox("Add every Team asset here. The Level Editor reads this list.")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true)]
        [AssetsOnly]
        public List<Team> teams = new();

        public int Count => teams?.Count ?? 0;

        public Team Get(int index)
        {
            if (teams == null || index < 0 || index >= teams.Count) return null;
            return teams[index];
        }
    }
}
