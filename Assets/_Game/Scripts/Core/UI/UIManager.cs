using Cysharp.Threading.Tasks;
using RubyCase.Core;
using UnityEngine;
using Zenject;

namespace RubyCase.UI
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        [SerializeField] private UIPanel loadingPanel;
        [SerializeField] private HUDPanel hudPanel;
        [SerializeField] private LevelCompletePanel levelCompletePanel;
        [SerializeField] private LevelFailPanel levelFailPanel;

        [Inject] private GameSettings _settings;
        [Inject] private ILevelManager _levelManager;

        private UIPanel _active;

        public async UniTask OnStateEnterAsync(GameState state)
        {
            UIPanel target = PanelFor(state);
            if (target == null) return;

            if (state == GameState.Playing)
                hudPanel.SetLevelDisplay(_levelManager.CurrentIndex);

            _active = target;
            await target.ShowAsync(_settings.PanelFadeDuration);
        }

        public async UniTask OnStateExitAsync(GameState state)
        {
            if (_active == null) return;
            await _active.HideAsync(_settings.PanelFadeDuration);
            _active = null;
        }

        private UIPanel PanelFor(GameState state) => state switch
        {
            GameState.Loading => loadingPanel,
            GameState.Playing => hudPanel,
            GameState.LevelComplete => levelCompletePanel,
            GameState.LevelFail => levelFailPanel,
            _ => null,
        };
    }
}
