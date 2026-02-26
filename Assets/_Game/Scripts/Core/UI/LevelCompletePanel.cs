using RubyCase.Core;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace RubyCase.UI
{
    public class LevelCompletePanel : UIPanel
    {
        [SerializeField] private Button _nextButton;

        [Inject] private LazyInject<IGameManager> _gameManager;

        protected override void Awake()
        {
            base.Awake();
            _nextButton?.onClick.AddListener(() => _gameManager.Value.NextLevel());
        }

        private void OnDestroy() => _nextButton?.onClick.RemoveAllListeners();
    }
}
