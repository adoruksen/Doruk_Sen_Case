using System;
using UnityEngine;

namespace RubyCase.Core.Session
{
    public sealed class CollectableManager : ICollectableManager
    {
        public int Total { get; private set; }
        public int Remaining { get; private set; }
        public bool IsCleared => Total > 0 && Remaining <= 0;

        public event Action Cleared;

        public void InitializeFromLevel(GameObject[] collectables)
        {
            Reset();
            Total = collectables?.Length ?? 0;
            Remaining = Total;

            if (Total == 0)
                Cleared?.Invoke();
        }

        public void MarkResolved(GameObject collectable)
        {
            if (collectable == null || Remaining <= 0) return;

            Remaining--;

            if (Remaining == 0)
                Cleared?.Invoke();
        }

        public void Reset()
        {
            Total = 0;
            Remaining = 0;
        }
    }
}