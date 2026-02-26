using System;
using Cysharp.Threading.Tasks;
using RubyCase.LevelSystem;

namespace RubyCase.Core
{
    public interface ILevelManager
    {
        LevelData CurrentData { get; }
        LevelContext CurrentContext { get; }
        ILevelSession CurrentSession { get; }
        int CurrentIndex { get; }

        event Action OnLevelReady;
        event Action OnLevelCleared;
        event Action<LevelData> OnSpawnRequested;

        UniTask LoadLevelAsync(int index);
        UniTask ReloadCurrentAsync();
        UniTask LoadNextAsync();
        void NotifySpawnComplete();
        void SetCurrentSession(ILevelSession session);
    }
}
