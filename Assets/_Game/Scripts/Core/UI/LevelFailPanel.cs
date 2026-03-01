using RubyCase.Core.GameLoop;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RubyCase.Core.UI
{
    public class LevelFailPanel : UIPanel
    {
        [SerializeField] private Button retryButton;

        [Inject] private LazyInject<IGameManager> _gameManager;

        protected override void Awake()
        {
            base.Awake();
            retryButton.onClick.AddListener(() => _gameManager.Value.RestartLevel());
        }

        private void OnDestroy() => retryButton.onClick.RemoveAllListeners();
    }
}
