using System;

namespace RubyCase.Core.GameLoop
{
    public interface IGameManager
    {
        GameState CurrentState { get; }
        event Action<GameState, GameState> OnStateChanged;

        void StartLevel(int index);
        void RestartLevel();
        void NextLevel();
        void NotifyLevelComplete();
        void NotifyLevelFail();
    }
}
