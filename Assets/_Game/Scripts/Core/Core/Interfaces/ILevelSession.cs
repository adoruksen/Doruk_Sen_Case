using System;

namespace RubyCase.Core
{
    public interface ILevelSession : IDisposable
    {
        int LevelIndex { get; }
        bool IsCompleted { get; }
        bool IsFailed { get; }

        IBoxManager Boxes { get; }
        ICollectableManager Collectables { get; }

        event Action Completed;
        event Action Failed;
    }
}
