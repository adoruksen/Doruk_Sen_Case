using System;
using UnityEngine;

namespace RubyCase.TeamSystem
{
    public class ModelTeamColorUpdater : MonoBehaviour
    {
        private IHaveTeam _teamOwner;
        private AdvancedMaterialSetter _setter;

        private void Awake()
        {
            _teamOwner = GetComponentInParent<IHaveTeam>();
            _setter = GetComponentInChildren<AdvancedMaterialSetter>();
        }

        private void OnEnable()
        {
            if (_teamOwner != null) _teamOwner.OnTeamChanged += _setter.ApplyTeam;
        }

        private void OnDisable()
        {
            if (_teamOwner != null) _teamOwner.OnTeamChanged -= _setter.ApplyTeam;
        }
    }
}
