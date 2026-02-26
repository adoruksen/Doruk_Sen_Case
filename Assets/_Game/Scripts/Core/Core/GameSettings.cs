using UnityEngine;
using Sirenix.OdinInspector;

namespace RubyCase.Core
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "RubyCase/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Title("Level")]
        public int StartLevelIndex = 0;

        [Title("UI")]
        public float PanelFadeDuration = 0.3f;
    }
}
