using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.TeamSystem
{
    [CreateAssetMenu(menuName = "RubyCase/TeamSystem/TeamDatabase", order = -398)]
    public class TeamDatabase : ScriptableObject
    {
        public List<Team> teams = new();

        public int Count => teams?.Count ?? 0;

        public Team Get(int index)
        {
            if (teams == null || index < 0 || index >= teams.Count) return null;
            return teams[index];
        }
    }
}
