using System;
using RubyCase.Core.Level;
using RubyCase.Core.Session;
using Zenject;

namespace RubyCase.Core.GameLoop
{
    public sealed class CoreLoopService : IInitializable, IDisposable
    {
        private readonly IGameManager _game;
        private readonly ILevelManager _levels;
        private readonly ILevelSessionFactory _sessionFactory;

        private ILevelSession _activeSession;

        public CoreLoopService(IGameManager game, ILevelManager levels, ILevelSessionFactory sessionFactory)
        {
            _game = game;
            _levels = levels;
            _sessionFactory = sessionFactory;
        }

        public void Initialize()
        {
            _levels.OnLevelReady += HandleLevelReady;
            _levels.OnLevelCleared += HandleLevelCleared;
        }

        public void Dispose()
        {
            _levels.OnLevelReady -= HandleLevelReady;
            _levels.OnLevelCleared -= HandleLevelCleared;
            DisposeActiveSession();
        }

        private void HandleLevelReady()
        {
            DisposeActiveSession();

            _activeSession = _sessionFactory.Create(_levels.CurrentIndex, _levels.CurrentContext);
            _levels.SetCurrentSession(_activeSession);

            _activeSession.Completed += OnSessionCompleted;
            _activeSession.Failed += OnSessionFailed;
        }

        private void HandleLevelCleared()
        {
            DisposeActiveSession();
            _levels.SetCurrentSession(null);
        }

        private void DisposeActiveSession()
        {
            if (_activeSession == null) return;

            _activeSession.Completed -= OnSessionCompleted;
            _activeSession.Failed -= OnSessionFailed;

            _activeSession.Dispose();
            _activeSession = null;
        }

        private void OnSessionCompleted() => _game.NotifyLevelComplete();
        private void OnSessionFailed() => _game.NotifyLevelFail();
    }
}
