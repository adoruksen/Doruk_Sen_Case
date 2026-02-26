using RubyCase.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RubyCase.UI
{
    public class LevelCompletePanel : UIPanel
    {
        [SerializeField] private Button nextButton;

        [Inject] private LazyInject<IGameManager> _gameManager;

        protected override void Awake()
        {
            base.Awake();
            nextButton.onClick.AddListener(() => _gameManager.Value.NextLevel());
        }

        private void OnDestroy() => nextButton.onClick.RemoveAllListeners();
    }
}
