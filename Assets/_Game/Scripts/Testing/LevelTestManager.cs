using RubyCase.Core.GameLoop;
using RubyCase.Core.Level;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace RubyCase.Testing
{
    public sealed class LevelTestManager : MonoBehaviour
    {
        [SerializeField] private int _startLevelIndex = 0;

        private IGameManager _gameManager;
        private ILevelManager _levelManager;

        [Inject]
        private void Construct(IGameManager gameManager, ILevelManager levelManager)
        {
            _gameManager = gameManager;
            _levelManager = levelManager;
        }

        [ShowInInspector, ReadOnly, FoldoutGroup("State")]
        private GameState CurrentState => _gameManager != null ? _gameManager.CurrentState : GameState.Idle;

        [ShowInInspector, ReadOnly, FoldoutGroup("State")]
        private int CurrentLevelIndex => _levelManager != null ? _levelManager.CurrentIndex : -1;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        private LevelContext Context => _levelManager != null ? _levelManager.CurrentContext : null;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        private int Boxes => Context != null ? Context.BoxesCount : 0;

        [ShowInInspector, ReadOnly, FoldoutGroup("Runtime")]
        private int Collectables => Context != null ? Context.CollectablesCount : 0;

        [Button(ButtonSizes.Medium), FoldoutGroup("Actions")]
        private void StartLevel()
        {
            if (_gameManager == null) return;
            _gameManager.StartLevel(_startLevelIndex);
        }

        [Button(ButtonSizes.Medium), FoldoutGroup("Actions")]
        private void Restart() => _gameManager?.RestartLevel();

        [Button(ButtonSizes.Medium), FoldoutGroup("Actions")]
        private void Next() => _gameManager?.NextLevel();

        [Button(ButtonSizes.Medium), FoldoutGroup("Actions")]
        private void ForceWin() => _gameManager?.NotifyLevelComplete();

        [Button(ButtonSizes.Medium), FoldoutGroup("Actions")]
        private void ForceFail() => _gameManager?.NotifyLevelFail();
    }
}