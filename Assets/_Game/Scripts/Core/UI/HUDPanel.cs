using TMPro;
using UnityEngine;

namespace RubyCase.UI
{
    public class HUDPanel : UIPanel
    {
        [SerializeField] private TextMeshProUGUI levelLabel;
        [SerializeField] private TextMeshProUGUI collectableCountLabel;

        public void SetLevelDisplay(int levelIndex)
        {
            if (levelLabel != null)
                levelLabel.text = $"Level {levelIndex + 1}";
        }
    }
}
