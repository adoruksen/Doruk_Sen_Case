using UnityEngine;

namespace RubyCase.TeamSystem
{
    [CreateAssetMenu(menuName = "RubyCase/TeamSystem/Team", order = -399)]
    public class Team : ScriptableObject
    {
        [Header("Runtime Colors")]
        [SerializeField] private Color boxColor;
        [SerializeField] private Color collectableColor;

        [Header("Editor")]
        [SerializeField] private Color editorColor;
        [SerializeField] private string editorSymbol = "●";

        public Color BoxColor => boxColor;
        public Color CollectableColor   => collectableColor;
        public Color EditorColor    => editorColor;
        public string EditorSymbol  => editorSymbol;
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            boxColor.a = 1f;
            collectableColor.a = 1f;
            editorColor.a = 1f;
        }
#endif
    }
}
