using System.Collections.Generic;
using UnityEngine;

namespace RubyCase.TeamSystem
{
    public class AdvancedMaterialSetter : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private List<MaterialPropertyColorBinding> bindings = new();

        private MaterialPropertyBlock _block;
        private Team _currentTeam;

        private void Awake()
        {
            if (targetRenderer == null) targetRenderer = GetComponent<Renderer>();
            _block = new MaterialPropertyBlock();
        }

        public void ApplyTeam(Team team)
        {
            if (team == null || targetRenderer == null) return;
            _currentTeam = team;
            targetRenderer.GetPropertyBlock(_block);
            foreach (var b in bindings)
                _block.SetColor(b.propertyName, ResolveColor(team, b.type));
            targetRenderer.SetPropertyBlock(_block);
        }
        

        public void Restore() { if (_currentTeam != null) ApplyTeam(_currentTeam); }

        private static Color ResolveColor(Team t, TeamMaterialGroup.Type type) => type switch
        {
            TeamMaterialGroup.Type.Box => t.BoxColor,
            TeamMaterialGroup.Type.Collectable => t.CollectableColor,
            _ => t.BoxColor
        };
    }
}
