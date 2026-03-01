using System;
using Cysharp.Threading.Tasks;
using Zenject;
using RubyCase.Core.UI;
using RubyCase.Core.Level;

namespace RubyCase.Core.GameLoop
{
    public class GameManager : IGameManager, IInitializable, IDisposable
    {
        public GameState CurrentState { get; private set; } = GameState.Idle;
        public event Action<GameState, GameState> OnStateChanged;

        private readonly ILevelManager _levelManager;
        private readonly IUIManager _uiManager;
        private readonly GameSettings _settings;

        private bool _transitioning;
        private Action _onLevelReadyHandler;

        public GameManager(ILevelManager levelManager, IUIManager uiManager, GameSettings settings)
        {
            _levelManager = levelManager;
            _uiManager = uiManager;
            _settings = settings;
        }

        public void Initialize()
        {
            _onLevelReadyHandler = () => HandleLevelReadyAsync().Forget();
            _levelManager.OnLevelReady += _onLevelReadyHandler;
            StartLevel(_settings.StartLevelIndex);
        }

        public void Dispose()
        {
            _levelManager.OnLevelReady -= _onLevelReadyHandler;
        }

        public void StartLevel(int index)
        {
            if (_transitioning) return;
            TransitionAsync(GameState.Loading, () => _levelManager.LoadLevelAsync(index)).Forget();
        }

        public void RestartLevel()
        {
            if (_transitioning) return;
            TransitionAsync(GameState.Loading, () => _levelManager.ReloadCurrentAsync()).Forget();
        }

        public void NextLevel()
        {
            if (_transitioning) return;
            TransitionAsync(GameState.Loading, () => _levelManager.LoadNextAsync()).Forget();
        }

        public void NotifyLevelComplete()
        {
            if (CurrentState != GameState.Playing) return;
            TransitionAsync(GameState.LevelComplete).Forget();
        }

        public void NotifyLevelFail()
        {
            if (CurrentState != GameState.Playing) return;
            TransitionAsync(GameState.LevelFail).Forget();
        }

        private async UniTaskVoid HandleLevelReadyAsync()
        {
            if (_transitioning)
                await UniTask.WaitUntil(() => !_transitioning);

            TransitionAsync(GameState.Playing).Forget();
        }

        private async UniTaskVoid TransitionAsync(GameState next, Func<UniTask> during = null)
        {
            if (_transitioning) return;
            _transitioning = true;

            var prev = CurrentState;

            await _uiManager.OnStateExitAsync(prev);

            if (during != null)
                await during();

            CurrentState = next;
            OnStateChanged?.Invoke(prev, next);

            await _uiManager.OnStateEnterAsync(next);

            _transitioning = false;
        }
    }
}