using System;
using RubyCase.TeamSystem;
using RubyCase.Pool;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class CollectableItemController : MonoBehaviour, IHaveTeam, IPoolable
    {
        public event Action<Team> OnTeamChanged;
        [ShowInInspector, ReadOnly] public Team Team { get; private set; }

        public void AssignTeam(Team team)
        {
            if (Team == team) return;
            Team = team;
            OnTeamChanged?.Invoke(Team);
        }

        public void OnSpawn()
        {

        }

        public void OnDespawn()
        {
            Team = null;
            OnTeamChanged = null;
        }
    }
}