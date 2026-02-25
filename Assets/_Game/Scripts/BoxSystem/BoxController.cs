using System;
using RubyCase.TeamSystem;
using Sirenix.OdinInspector;
using UnityEngine;

namespace RubyCase.BoxSystem
{
    public class BoxController : MonoBehaviour,IHaveTeam
    {
        public event Action<Team> OnTeamChanged;
        [ShowInInspector,ReadOnly] public Team Team { get; private set; }

        public void AssignTeam(Team team)
        {
            if(Team == team) return;
            Team = team;
            OnTeamChanged?.Invoke(Team);
        }
    }

}
