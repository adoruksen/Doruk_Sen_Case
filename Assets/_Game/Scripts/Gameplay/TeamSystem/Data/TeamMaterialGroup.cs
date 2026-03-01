using System;
using UnityEngine;

namespace RubyCase.TeamSystem
{
    public class TeamMaterialGroup
    {
        public enum Type { Box, Collectable }

        private readonly Material boxMat;
        private readonly Material collectableMat;

        public TeamMaterialGroup(Team team, Material boxBase, Material collectableBase)
        {
            boxMat = new Material(boxBase) { color = team.BoxColor };
            collectableMat   = new Material(collectableBase)   { color = team.CollectableColor };
        }

        public Material GetByType(Type type) => type switch
        {
            Type.Box => boxMat,
            Type.Collectable => collectableMat,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}
