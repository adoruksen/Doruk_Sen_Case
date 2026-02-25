using System;
using UnityEngine;

namespace RubyCase.TeamSystem
{
    [Serializable]
    public class MaterialPropertyColorBinding
    {
        [Tooltip("Shader property name, e.g. _BaseColor")]
        public string propertyName;
        public TeamMaterialGroup.Type type;
    }
}
