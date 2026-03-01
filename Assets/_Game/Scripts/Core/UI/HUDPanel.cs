using TMPro;
using UnityEngine;

namespace RubyCase.UI
{
    public class HUDPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI levelLabel;

        public void SetLevelDisplay(int levelIndex)
        {
            if (levelLabel != null)
                levelLabel.text = $"Level {levelIndex + 1}";
        }
    }
}
