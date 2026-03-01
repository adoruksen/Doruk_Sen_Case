using System;
using RubyCase.Core.Level;

namespace RubyCase.Core.Session
{
    public sealed class LevelSession : ILevelSession
    {
        public int LevelIndex { get; }
        public bool IsCompleted { get; private set; }
        public bool IsFailed { get; private set; }

        public IBoxManager Boxes { get; }
        public ICollectableManager Collectables { get; }

        public event Action Completed;
        public event Action Failed;

        public LevelSession(int levelIndex, IBoxManager boxes, ICollectableManager collectables)
        {
            LevelIndex = levelIndex;
            Boxes = boxes ?? throw new ArgumentNullException(nameof(boxes));
            Collectables = collectables ?? throw new ArgumentNullException(nameof(collectables));
            Collectables.Cleared += TryComplete;
        }

        public void Initialize(LevelContext context)
        {
            Boxes.InitializeFromLevel(context?.Boxes?.ToArray());
            Collectables.InitializeFromLevel(context?.Collectables?.ToArray());
            TryComplete();
        }

        public void Fail()
        {
            if (IsCompleted || IsFailed) return;
            IsFailed = true;
            Failed?.Invoke();
        }

        private void TryComplete()
        {
            if (IsCompleted || IsFailed || !Collectables.IsCleared) return;
            IsCompleted = true;
            Completed?.Invoke();
        }

        public void Dispose()
        {
            Collectables.Cleared -= TryComplete;
            Boxes.Reset();
            Collectables.Reset();
        }
    }
}