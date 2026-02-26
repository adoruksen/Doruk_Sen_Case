using System;
using System.Linq;
using UnityEngine;

namespace RubyCase.Core
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

            Boxes.Cleared += EvaluateCompletion;
            Collectables.Cleared += EvaluateCompletion;
        }

        public void Initialize(LevelContext context)
        {
            Boxes.InitializeFromLevel(context?.Boxes?.ToArray());
            Collectables.InitializeFromLevel(context?.Collectables?.ToArray());

            EvaluateCompletion();
        }

        public void Fail()
        {
            if (IsCompleted || IsFailed) return;
            IsFailed = true;
            Failed?.Invoke();
        }

        private void EvaluateCompletion()
        {
            if (IsCompleted || IsFailed) return;
            if (!Boxes.IsCleared) return;
            if (!Collectables.IsCleared) return;

            IsCompleted = true;
            Completed?.Invoke();
        }

        public void Dispose()
        {
            Boxes.Cleared -= EvaluateCompletion;
            Collectables.Cleared -= EvaluateCompletion;

            Boxes.Reset();
            Collectables.Reset();
        }
    }
}
