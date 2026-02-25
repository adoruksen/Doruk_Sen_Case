using System;

namespace RubyCase.TeamSystem
{
    public interface IHaveTeam
    {
        event Action<Team> OnTeamChanged;
        Team Team { get; }
        void AssignTeam(Team team);
    }
}
