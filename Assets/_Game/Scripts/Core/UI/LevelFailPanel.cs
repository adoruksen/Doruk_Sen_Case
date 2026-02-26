using RubyCase.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RubyCase.UI
{
    public class LevelFailPanel : UIPanel
    {
        [SerializeField] private Button _retryButton;

        [Inject] private LazyInject<IGameManager> _gameManager;

        protected override void Awake()
        {
            base.Awake();
            _retryButton?.onClick.AddListener(() => _gameManager.Value.RestartLevel());
        }

        private void OnDestroy() => _retryButton?.onClick.RemoveAllListeners();
    }
}
